import { useEffect, useRef, useState } from 'react';
import { Alert, Button, Card, Container, Stack, Title } from '@mantine/core';
import { scan } from '../../api/inspection';
import { asApiError } from '../../api/problemDetails';
import type { ApiError } from '../../api/problemDetails';
import type { InspectionResponse } from '../../types/contracts';
import type { SupportedFruit } from '../../types/enums';
import { ProblemAlert } from '../../components/ProblemAlert';
import { FruitSelect } from './FruitSelect';
import { ImageDropzone } from './ImageDropzone';
import { InspectionResult } from './InspectionResult';

/**
 * ScanRequest.FruitType is a non-nullable enum with no [Required], so omitting
 * it would silently bind to Banana on the server. Preselecting it here means
 * what the operator sees is what gets sent.
 */
const DEFAULT_FRUIT: SupportedFruit = 'Banana';

/** The chosen file together with the object URL rendered as its preview. */
interface Selection {
  file: File;
  url: string;
}

export function InspectionPage() {
  const [fruitType, setFruitType] = useState<SupportedFruit>(DEFAULT_FRUIT);
  const [selection, setSelection] = useState<Selection | null>(null);
  const [fileProblem, setFileProblem] = useState<string | null>(null);
  const [result, setResult] = useState<InspectionResponse | null>(null);
  const [error, setError] = useState<ApiError | null>(null);
  const [scanning, setScanning] = useState(false);

  // The live object URL, mirrored in a ref so it can be revoked from an event
  // handler. Revoking from an effect that depends on the selection would break
  // the preview under StrictMode, which runs setup, cleanup, setup on mount.
  const previewUrlRef = useRef<string | null>(null);

  useEffect(
    () => () => {
      if (previewUrlRef.current !== null) {
        URL.revokeObjectURL(previewUrlRef.current);
      }
    },
    [],
  );

  const replaceSelection = (next: File | null) => {
    if (previewUrlRef.current !== null) {
      URL.revokeObjectURL(previewUrlRef.current);
      previewUrlRef.current = null;
    }

    if (next === null) {
      setSelection(null);
      return;
    }

    const url = URL.createObjectURL(next);
    previewUrlRef.current = url;
    setSelection({ file: next, url });
  };

  const acceptFile = (accepted: File) => {
    replaceSelection(accepted);
    setFileProblem(null);
    setResult(null);
    setError(null);
  };

  const rejectFile = (message: string) => {
    replaceSelection(null);
    setFileProblem(message);
    setResult(null);
    setError(null);
  };

  const runScan = async () => {
    if (selection === null) {
      setFileProblem('An image file is required.');
      return;
    }

    setScanning(true);
    setResult(null);
    setError(null);

    try {
      setResult(await scan({ image: selection.file, fruitType }));
    } catch (cause) {
      // A 401 here means the token expired. api/client.ts has already cleared
      // the session, which re-renders RequireAuth and routes to /login.
      setError(asApiError(cause));
    } finally {
      setScanning(false);
    }
  };

  return (
    <Container size="sm">
      <Stack gap="lg">
        <Title order={2}>Inspection</Title>

        <Card withBorder padding="lg" radius="md">
          <Stack gap="md">
            <FruitSelect value={fruitType} onChange={setFruitType} disabled={scanning} />

            <ImageDropzone
              file={selection?.file ?? null}
              previewUrl={selection?.url ?? null}
              onFileAccepted={acceptFile}
              onFileRejected={rejectFile}
              disabled={scanning}
            />

            {fileProblem !== null && (
              <Alert color="red" variant="light" title="Image rejected" role="alert">
                {fileProblem}
              </Alert>
            )}

            <Button
              onClick={() => void runScan()}
              loading={scanning}
              disabled={selection === null}
            >
              Run inspection
            </Button>
          </Stack>
        </Card>

        {error !== null && <ProblemAlert error={error} handledFields={['Image', 'FruitType']} />}

        {result !== null && <InspectionResult result={result} />}
      </Stack>
    </Container>
  );
}
