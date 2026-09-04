import type { ReactNode } from 'react';
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
  const Shape = SHAPES[fruit];

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
      <Shape color={color} speckles={speckles} />
    </svg>
  );
}

interface ShapeProps {
  color: string;
  speckles: number;
}

/**
 * One drawing per fruit, looked up rather than branched. A new fruit adds an
 * entry here and the type stops compiling until it does.
 */
const SHAPES: Record<SupportedFruit, (props: ShapeProps) => ReactNode> = {
  Banana,
  Avocado,
  Pear,
  Mango,
};

/**
 * Bruising and spotting, spread over the fruit's body. The positions come from
 * the index rather than a random seed so a stage looks the same on every
 * render — the guide would otherwise reshuffle its own illustrations.
 */
function Speckles({
  count,
  x,
  y,
  spread,
  fill,
}: {
  count: number;
  x: number;
  y: number;
  spread: number;
  fill: string;
}) {
  return (
    <>
      {Array.from({ length: count }, (_, index) => (
        <ellipse
          key={index}
          cx={x + ((index * 7) % spread)}
          cy={y + ((index * 13) % 17)}
          rx={2.4}
          ry={1.7}
          fill={fill}
          opacity={0.5}
          transform={`rotate(${String(index * 24)} ${String(x + ((index * 7) % spread))} ${String(y + ((index * 13) % 17))})`}
        />
      ))}
    </>
  );
}

function Banana({ color, speckles }: ShapeProps) {
  return (
    <g>
      <path
        d="M22 24c-4 26 10 48 36 54 12 3 22 1 26-4 3-4 1-8-4-9-19-3-33-13-40-28-4-9-5-16-4-22 1-5-1-8-5-8-4 0-8 4-9 17Z"
        fill={color}
      />
      {/* Stem and tip, always darker so the silhouette reads at small sizes. */}
      <path d="M20 24c1-8 4-13 9-13 3 0 5 2 4 5-3 6-5 11-5 16Z" fill="#6b5a2e" opacity="0.75" />
      <path d="M80 68c5 1 7 5 4 9-2 3-6 4-9 3 3-3 5-8 5-12Z" fill="#6b5a2e" opacity="0.55" />
      <Speckles count={speckles} x={34} y={44} spread={48} fill="#4e342e" />
    </g>
  );
}

function Avocado({ color }: ShapeProps) {
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

/**
 * The narrow neck is the point of the drawing: it is the part a shopper is told
 * to press, and it is what separates a pear from an apple at glyph size.
 */
function Pear({ color, speckles }: ShapeProps) {
  return (
    <g>
      <ellipse cx="50" cy="70" rx="26" ry="25" fill={color} />
      <ellipse cx="50" cy="43" rx="17" ry="18" fill={color} />
      <path
        d="M50 26c0-9 3-14 9-16"
        fill="none"
        stroke="#6b5a2e"
        strokeOpacity="0.8"
        strokeWidth="4"
        strokeLinecap="round"
      />
      <path
        d="M31 56c-3-10 2-19 11-22"
        fill="none"
        stroke="#ffffff"
        strokeOpacity="0.2"
        strokeWidth="3"
        strokeLinecap="round"
      />
      <Speckles count={speckles} x={36} y={60} spread={28} fill="#5d4326" />
    </g>
  );
}

/**
 * An ovoid that is wider at the shoulder than at the beak, lying on its side.
 *
 * A mango drawn upright and symmetric is a rounded blob and reads as an apple
 * at glyph size; drawn as a pointed lens it reads as a leaf. The identifying
 * features are the long axis and the two unequal ends, so the outline is built
 * around those rather than around a tilted ellipse.
 */
function Mango({ color, speckles }: ShapeProps) {
  return (
    <g>
      <path
        d="M17 55c0-13 12-24 30-26 18-2 33 6 35 18 2 13-11 25-29 27-18 2-36-6-36-19Z"
        fill={color}
      />
      {/* The cheek catches the light; a mango is glossy where a pear is matt. */}
      <path
        d="M31 44c6-8 16-12 26-11"
        fill="none"
        stroke="#ffffff"
        strokeOpacity="0.28"
        strokeWidth="5"
        strokeLinecap="round"
      />
      {/* The stem sits on the broad shoulder, which is the end you smell. */}
      <path
        d="M76 33c3-4 7-5 10-3"
        fill="none"
        stroke="#6b5a2e"
        strokeOpacity="0.8"
        strokeWidth="4"
        strokeLinecap="round"
      />
      <Speckles count={speckles} x={34} y={50} spread={34} fill="#3a2417" />
    </g>
  );
}
