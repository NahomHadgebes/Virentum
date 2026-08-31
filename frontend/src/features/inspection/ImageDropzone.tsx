import { Dropzone } from '@mantine/dropzone';
import { Group, Image, Stack, Text } from '@mantine/core';
import {
  ALLOWED_IMAGE_CONTENT_TYPES,
  MAX_IMAGE_BYTES,
  validateImage,
} from '../../validation/image';

interface ImageDropzoneProps {
  file: File | null;
  /** Preview object URL, owned by the parent so it can be revoked. */
  previewUrl: string | null;
  /** Called with the file when it passes the backend's rules, null otherwise. */
  onFileAccepted: (file: File) => void;
  onFileRejected: (message: string) => void;
  disabled: boolean;
}

/**
 * Dropzone's own accept/maxSize props only filter the file picker. Every file
 * that gets through — dropped or picked — is checked again by validateImage so
 * the message the operator reads is the one the API would have produced.
 * Rejections from the dropzone itself go through the same check rather than
 * being dropped on the floor.
 */
export function ImageDropzone({
  file,
  previewUrl,
  onFileAccepted,
  onFileRejected,
  disabled,
}: ImageDropzoneProps) {
  const handle = (candidate: File | undefined) => {
    if (candidate === undefined) {
      onFileRejected('An image file is required.');
      return;
    }

    const problem = validateImage(candidate);
    if (problem === null) {
      onFileAccepted(candidate);
    } else {
      onFileRejected(problem);
    }
  };

  return (
    <Dropzone
      onDrop={(files) => {
        handle(files[0]);
      }}
      onReject={(rejections) => {
        handle(rejections[0]?.file);
      }}
      accept={[...ALLOWED_IMAGE_CONTENT_TYPES]}
      maxSize={MAX_IMAGE_BYTES}
      maxFiles={1}
      multiple={false}
      disabled={disabled}
    >
      <Group justify="center" mih={160} style={{ pointerEvents: 'none' }}>
        {previewUrl !== null ? (
          <Stack align="center" gap="xs">
            <Image src={previewUrl} alt="Selected produce" mah={160} fit="contain" radius="sm" />
            <Text size="sm">{file?.name}</Text>
            <Text size="xs" c="dimmed">
              Drop another image to replace it
            </Text>
          </Stack>
        ) : (
          <Stack align="center" gap={4}>
            <Text size="sm" fw={500}>
              Drop a produce photo here, or click to choose one
            </Text>
            <Text size="xs" c="dimmed">
              JPEG, PNG or WebP · up to {MAX_IMAGE_BYTES / (1024 * 1024)} MB
            </Text>
          </Stack>
        )}
      </Group>
    </Dropzone>
  );
}
