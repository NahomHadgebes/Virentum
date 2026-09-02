import type { SupportedFruit } from '../../types/enums';

interface FruitGlyphProps {
  fruit: SupportedFruit;
  /** Skin colour for this ripeness stage. */
  color: string;
  size?: number;
  /**
   * Spots and speckling grow with ripeness, so the drawing changes shape and
   * not only hue — the stage is legible without relying on colour alone.
   */
  ripeness?: number;
  title?: string;
}

/**
 * Drawn rather than photographed.
 *
 * A stock photo of a banana is one banana under one light; a glyph can take the
 * exact skin colour a stage declares, stay crisp at any size, weigh nothing, and
 * carry no licence. It also keeps the guide honest: an illustration reads as a
 * diagram of a stage, where a photograph would imply "your fruit will look like
 * this".
 */
export function FruitGlyph({ fruit, color, size = 72, ripeness = 50, title }: FruitGlyphProps) {
  const speckles = ripeness > 70 ? Math.min(9, Math.round((ripeness - 70) / 3.5)) : 0;

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      role={title === undefined ? 'presentation' : 'img'}
      aria-label={title}
      aria-hidden={title === undefined}
    >
      {title !== undefined && <title>{title}</title>}
      {fruit === 'Banana' ? <Banana color={color} speckles={speckles} /> : <Avocado color={color} />}
    </svg>
  );
}

function Banana({ color, speckles }: { color: string; speckles: number }) {
  return (
    <g>
      <path
        d="M22 24c-4 26 10 48 36 54 12 3 22 1 26-4 3-4 1-8-4-9-19-3-33-13-40-28-4-9-5-16-4-22 1-5-1-8-5-8-4 0-8 4-9 17Z"
        fill={color}
      />
      {/* Stem and tip, always darker so the silhouette reads at small sizes. */}
      <path d="M20 24c1-8 4-13 9-13 3 0 5 2 4 5-3 6-5 11-5 16Z" fill="#6b5a2e" opacity="0.75" />
      <path d="M80 68c5 1 7 5 4 9-2 3-6 4-9 3 3-3 5-8 5-12Z" fill="#6b5a2e" opacity="0.55" />
      {Array.from({ length: speckles }, (_, index) => (
        <ellipse
          key={index}
          cx={34 + index * 5.4}
          cy={44 + ((index * 13) % 17)}
          rx={2.4}
          ry={1.7}
          fill="#4e342e"
          opacity={0.5}
          transform={`rotate(${String(index * 24)} ${String(34 + index * 5.4)} ${String(44 + ((index * 13) % 17))})`}
        />
      ))}
    </g>
  );
}

function Avocado({ color }: { color: string }) {
  return (
    <g>
      <path
        d="M50 12c9 0 15 7 17 15 2 8 9 14 9 26 0 17-12 31-26 31S24 70 24 53c0-12 7-18 9-26 2-8 8-15 17-15Z"
        fill={color}
      />
      {/* A lighter rim reads as the curve of the skin without a gradient. */}
      <path
        d="M50 12c9 0 15 7 17 15 2 8 9 14 9 26 0 17-12 31-26 31"
        fill="none"
        stroke="#ffffff"
        strokeOpacity="0.22"
        strokeWidth="3"
        strokeLinecap="round"
      />
      <path d="M48 12c1-4 3-6 5-6s3 3 1 6Z" fill="#6b5a2e" opacity="0.7" />
    </g>
  );
}
