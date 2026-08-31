using Microsoft.Extensions.Logging.Abstractions;
using Virentum.Api.Domain.Enums;
using Virentum.Api.Domain.Processors;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Services.Inspection;
using Virentum.Api.Tests.TestDoubles;
using Xunit;

namespace Virentum.Api.Tests.Services.Inspection;

public sealed class InspectionHistoryAndSummaryTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);

    private readonly RecordingInspectionRepository _repository = new();

    private InspectionService CreateService() =>
        new(
            new StubVisionService(0.5),
            new FruitProcessorFactory(new IFruitProcessor[] { new BananaProcessor(), new AvocadoProcessor() }),
            _repository,
            new FixedTimeProvider(Now),
            NullLogger<InspectionService>.Instance);

    private void Given(
        string storeId,
        SupportedFruit fruit,
        int ripeness,
        CommercialStatus status,
        TimeSpan ago)
    {
        _repository.Saved.Add(new InspectionRecord
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            FruitType = fruit,
            RipenessPercent = ripeness,
            CommercialStatus = status,
            Recommendation = "Recorded earlier.",
            ScannedAt = Now - ago,
        });
    }

    [Fact]
    public async Task History_returns_nothing_for_a_store_that_has_never_scanned()
    {
        var history = await CreateService().GetHistoryAsync("quiet-store", 20);

        Assert.Empty(history);
    }

    [Fact]
    public async Task History_returns_newest_first()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(3));
        Given("demo-store", SupportedFruit.Avocado, 90, CommercialStatus.ActionRequired, TimeSpan.FromHours(1));

        var history = await CreateService().GetHistoryAsync("demo-store", 20);

        Assert.Equal(SupportedFruit.Avocado, history[0].FruitType);
        Assert.Equal(SupportedFruit.Banana, history[1].FruitType);
    }

    [Fact]
    public async Task History_never_leaks_another_store()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));
        Given("other-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));

        var history = await CreateService().GetHistoryAsync("demo-store", 20);

        Assert.Single(history);
    }

    [Fact]
    public async Task History_respects_the_limit()
    {
        for (var i = 0; i < 5; i++)
        {
            Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(i + 1));
        }

        var history = await CreateService().GetHistoryAsync("demo-store", 3);

        Assert.Equal(3, history.Count);
    }

    [Fact]
    public async Task History_carries_the_record_id_the_client_keys_on()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));

        var history = await CreateService().GetHistoryAsync("demo-store", 20);

        Assert.NotEqual(Guid.Empty, history[0].Id);
    }

    [Fact]
    public async Task Summary_counts_only_scans_inside_the_window()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromDays(2));
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromDays(30));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(1, summary.TotalScans);
        Assert.Equal(7, summary.WindowDays);
        Assert.Equal(Now.AddDays(-7), summary.Since);
    }

    /// <summary>
    /// The client charts a fixed set of categories, so a status that did not
    /// occur has to arrive as an explicit zero rather than a missing key.
    /// </summary>
    [Fact]
    public async Task Summary_lists_every_status_even_when_none_occurred()
    {
        Given("demo-store", SupportedFruit.Banana, 80, CommercialStatus.ActionRequired, TimeSpan.FromHours(1));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(Enum.GetValues<CommercialStatus>().Length, summary.ByStatus.Count);
        Assert.Equal(1, summary.ByStatus.Single(entry => entry.CommercialStatus == CommercialStatus.ActionRequired).Count);
        Assert.Equal(0, summary.ByStatus.Single(entry => entry.CommercialStatus == CommercialStatus.Expired).Count);
    }

    [Fact]
    public async Task Summary_lists_statuses_in_enum_declaration_order()
    {
        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(
            Enum.GetValues<CommercialStatus>(),
            summary.ByStatus.Select(entry => entry.CommercialStatus).ToArray());
    }

    [Fact]
    public async Task Summary_lists_every_fruit_even_when_none_occurred()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(Enum.GetValues<SupportedFruit>().Length, summary.ByFruit.Count);
        Assert.Equal(0, summary.ByFruit.Single(entry => entry.FruitType == SupportedFruit.Avocado).Count);
    }

    [Fact]
    public async Task Summary_averages_ripeness_across_the_window()
    {
        Given("demo-store", SupportedFruit.Banana, 40, CommercialStatus.Underripe, TimeSpan.FromHours(1));
        Given("demo-store", SupportedFruit.Banana, 60, CommercialStatus.ReadyForSale, TimeSpan.FromHours(2));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(50d, summary.AverageRipenessPercent);
    }

    /// <summary>
    /// Zero would read as "completely unripe". No data has to be distinguishable
    /// from a measurement of zero.
    /// </summary>
    [Fact]
    public async Task Summary_reports_no_average_when_nothing_was_scanned()
    {
        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(0, summary.TotalScans);
        Assert.Null(summary.AverageRipenessPercent);
        Assert.Null(summary.LastScanAt);
    }

    [Fact]
    public async Task Summary_reports_the_most_recent_scan()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromDays(3));
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(2));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(Now.AddHours(-2), summary.LastScanAt);
    }

    [Fact]
    public async Task Summary_never_counts_another_store()
    {
        Given("demo-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));
        Given("other-store", SupportedFruit.Banana, 50, CommercialStatus.ReadyForSale, TimeSpan.FromHours(1));

        var summary = await CreateService().GetSummaryAsync("demo-store", days: 7);

        Assert.Equal(1, summary.TotalScans);
    }
}
