import { ActionIcon, Box, Group, Stack, Text } from '@mantine/core';
import { Dropzone } from '@mantine/dropzone';
import {
  ALLOWED_IMAGE_CONTENT_TYPES,
  MAX_IMAGE_BYTES,
  MAX_IMAGES,
  validateImage,
} from '../../validation/image';
import classes from './ImageUploader.module.css';

/** A chosen file together with the object URL rendered as its thumbnail. */
export interface Shot {
  id: string;
  file: File;
  url: string;
}

interface ImageUploaderProps {
  shots: readonly Shot[];
  onAdd: (files: File[]) => void;
  onRemove: (id: string) => void;
  onReject: (message: string) => void;
  disabled: boolean;
}

/**
 * Up to three photographs of the same item.
 *
 * The first is required and the rest are genuinely optional, so the empty state
 * asks for one picture and the filled state suggests — rather than demands —
 * the two that make the reading stronger. Dropzone's own accept and maxSize
 * props only filter the file picker, so every file is checked again by
 * validateImage and rejections are routed through the same path instead of
 * being dropped on the floor.
 */
export function ImageUploader({ shots, onAdd, onRemove, onReject, disabled }: ImageUploaderProps) {
  const remaining = MAX_IMAGES - shots.length;

  const handle = (files: readonly File[]) => {
    if (files.length === 0) {
      onReject('An image file is required.');
      return;
    }

    const accepted: File[] = [];
    for (const file of files.slice(0, remaining)) {
      const problem = validateImage(file);
      if (problem === null) {
        accepted.push(file);
      } else {
        onReject(problem);
        return;
      }
    }

    if (files.length > remaining) {
      onReject(
        `Virentum reads up to ${String(MAX_IMAGES)} photographs of one item; the extra ones were not added.`,
      );
    }

    if (accepted.length > 0) {
      onAdd(accepted);
    }
  };

  return (
    <Stack gap="sm">
      <div className={classes.grid}>
        {shots.map((shot, index) => (
          <figure key={shot.id} className={`${classes.shot} rise`}>
            <img src={shot.url} alt={`Upload ${String(index + 1)}: ${shot.file.name}`} />
            <ActionIcon
              className={classes.remove}
              size="sm"
              radius="xl"
              variant="filled"
              color="dark"
              aria-label={`Remove image ${String(index + 1)}`}
              onClick={() => {
                onRemove(shot.id);
              }}
              disabled={disabled}
            >
              <RemoveIcon />
            </ActionIcon>
            <figcaption>{index === 0 ? 'Main view' : `Angle ${String(index + 1)}`}</figcaption>
          </figure>
        ))}

        {remaining > 0 && (
          <Dropzone
            onDrop={handle}
            onReject={(rejections) => {
              handle(rejections.map((rejection) => rejection.file));
            }}
            accept={[...ALLOWED_IMAGE_CONTENT_TYPES]}
            maxSize={MAX_IMAGE_BYTES}
            maxFiles={remaining}
            multiple={remaining > 1}
            disabled={disabled}
            className={shots.length === 0 ? classes.dropFirst : classes.dropMore}
          >
            <Stack align="center" justify="center" gap={6} style={{ pointerEvents: 'none' }} h="100%">
              <PlusIcon />
              {shots.length === 0 ? (
                <>
                  <Text fw={600} fz="sm">
                    Add a photo of the fruit
                  </Text>
                  <Text fz="xs" c="dimmed" ta="center">
                    JPEG, PNG or WebP · up to {MAX_IMAGE_BYTES / (1024 * 1024)} MB
                  </Text>
                </>
              ) : (
                <Text fz="xs" c="dimmed" ta="center">
                  Add another angle
                </Text>
              )}
            </Stack>
          </Dropzone>
        )}
      </div>

      {shots.length > 0 && shots.length < MAX_IMAGES && (
        <Group gap={6} wrap="nowrap" align="flex-start">
          <Box className={classes.hintDot} aria-hidden />
          <Text fz="xs" c="dimmed">
            Optional, but it helps: a second angle, or the inside if you have already cut it.
            Virentum pools the colour across every photo instead of judging one.
          </Text>
        </Group>
      )}
    </Stack>
  );
}

function PlusIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
      <path d="M12 5v14M5 12h14" />
    </svg>
  );
}

function RemoveIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" aria-hidden="true">
      <path d="M6 6l12 12M18 6L6 18" />
    </svg>
  );
}
