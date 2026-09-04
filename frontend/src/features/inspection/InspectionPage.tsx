import { useEffect, useRef, useState } from 'react';
import { Alert, Box, Button, Stack, Text, Title } from '@mantine/core';
import { useDocumentTitle, useScrollIntoView } from '@mantine/hooks';
import { scan } from '../../api/inspection';
import { asApiError } from '../../api/problemDetails';
import type { ApiError } from '../../api/problemDetails';
import type { InspectionResponse } from '../../types/contracts';
import type { SupportedFruit } from '../../types/enums';
import { useAudience } from '../../audience/useAudience';
import { ProblemAlert } from '../../components/ProblemAlert';
import { FruitSelect } from './FruitSelect';
import { ImageUploader } from './ImageUploader';
import type { Shot } from './ImageUploader';
import { InspectionResult } from './InspectionResult';
import { AnalysingCard } from './AnalysingCard';

/**
 * ScanRequest.FruitType is a non-nullable enum with no [Required], so omitting
 * it would silently bind to Banana on the server. Preselecting it means what the
 * operator sees is what gets sent.
 */
const DEFAULT_FRUIT: SupportedFruit = 'Banana';

let shotSequence = 0;

export function InspectionPage() {
  useDocumentTitle('Scan · Virentum');

  const { audience } = useAudience();
  const isConsumer = audience !== 'Business';

  const [fruitType, setFruitType] = useState<SupportedFruit>(DEFAULT_FRUIT);
  const [shots, setShots] = useState<Shot[]>([]);
  const [fileProblem, setFileProblem] = useState<string | null>(null);
  const [result, setResult] = useState<InspectionResponse | null>(null);
  const [scannedShots, setScannedShots] = useState<Shot[]>([]);
  const [error, setError] = useState<ApiError | null>(null);
  const [scanning, setScanning] = useState(false);

  // Object URLs live as long as their thumbnails do, so they are revoked from
  // the handler that removes one and once more when the page unmounts. An
  // effect keyed on the list would revoke them mid-render under StrictMode.
  const liveUrls = useRef<Set<string>>(new Set());
  useEffect(
    () => () => {
      for (const url of liveUrls.current) {
        URL.revokeObjectURL(url);
      }
    },
    [],
  );

  const { scrollIntoView, targetRef } = useScrollIntoView<HTMLDivElement>({ offset: 90 });

  const invalidate = () => {
    setResult(null);
    setError(null);
  };

  const addShots = (files: File[]) => {
    const added = files.map((file) => {
      const url = URL.createObjectURL(file);
      liveUrls.current.add(url);
      shotSequence += 1;
      return { id: `shot-${String(shotSequence)}`, file, url };
    });

    setShots((previous) => [...previous, ...added]);
    setFileProblem(null);
    invalidate();
  };

  const removeShot = (id: string) => {
    setShots((previous) => {
      const shot = previous.find((candidate) => candidate.id === id);
      if (shot !== undefined) {
        URL.revokeObjectURL(shot.url);
        liveUrls.current.delete(shot.url);
      }
      return previous.filter((candidate) => candidate.id !== id);
    });
    setFileProblem(null);
    invalidate();
  };

  /**
   * Changing the fruit invalidates the verdict on screen: it was reached with
   * the previous selection's thresholds and advice. The photographs are kept —
   * the operator is most likely about to re-run them.
   */
  const changeFruit = (next: SupportedFruit) => {
    setFruitType(next);
    invalidate();
  };

  const runScan = async () => {
    if (shots.length === 0) {
      setFileProblem('At least one image is required.');
      return;
    }

    setScanning(true);
    invalidate();

    try {
      const response = await scan({
        images: shots.map((shot) => shot.file),
        fruitType,
        audience: audience ?? 'Consumer',
      });
      setResult(response);
      setScannedShots(shots);
      scrollIntoView();
    } catch (cause) {
      // A 401 here means the token expired. api/client.ts has already cleared
      // the session, which re-renders RequireAuth and routes to /login.
      setError(asApiError(cause));
    } finally {
      setScanning(false);
    }
  };

  return (
      <Stack gap="xl">
        <Stack gap={6}>
          <Title order={2}>{isConsumer ? 'Check your fruit' : 'Inspect produce'}</Title>
          <Text c="dimmed">
            {isConsumer
              ? 'Take a photo, pick the fruit, and find out where it is on its ripeness scale.'
              : 'Photograph the item, pick the fruit, and record the shelf decision against this store.'}
          </Text>
        </Stack>

        {/* The form keeps a comfortable measure while the heading and the
            verdict below it use the full column: a select and a submit button
            stretched to 860px read as a stretched layout, not a wide one. */}
        <Box className="rise" maw={620}>
          <Stack gap="lg">
            <FruitSelect value={fruitType} onChange={changeFruit} disabled={scanning} />

            <ImageUploader
              shots={shots}
              onAdd={addShots}
              onRemove={removeShot}
              onReject={(message) => {
                setFileProblem(message);
              }}
              disabled={scanning}
            />

            {fileProblem !== null && (
              <Alert color="red" variant="light" title="Image not added" role="alert">
                {fileProblem}
              </Alert>
            )}

            <Button
              size="md"
              onClick={() => void runScan()}
              loading={scanning}
              disabled={shots.length === 0}
              fullWidth
            >
              {scanning
                ? 'Reading the colour…'
                : shots.length > 1
                  ? `Analyse ${String(shots.length)} photos`
                  : 'Analyse photo'}
            </Button>
          </Stack>
        </Box>

        {error !== null && <ProblemAlert error={error} />}

        <div ref={targetRef}>
          {scanning && <AnalysingCard imageCount={shots.length} />}
          {!scanning && result !== null && (
            <InspectionResult result={result} shots={scannedShots} />
          )}
        </div>
      </Stack>
  );
}
