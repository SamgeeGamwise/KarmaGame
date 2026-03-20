from __future__ import annotations

import math
import warnings
from dataclasses import dataclass
from pathlib import Path

from PIL import Image


TILE_SIZE = 32
MAX_DIMENSION = 4096
PART_COLUMNS = MAX_DIMENSION // TILE_SIZE
PART_ROWS = MAX_DIMENSION // TILE_SIZE
TILES_PER_PART = PART_COLUMNS * PART_ROWS

Image.MAX_IMAGE_PIXELS = None
warnings.simplefilter("ignore", Image.DecompressionBombWarning)


@dataclass(frozen=True)
class SplitJob:
    source_name: str
    output_pattern: str
    explicit_part_tile_counts: tuple[int, ...] | None = None
    output_start_index: int = 1


JOBS = [
    SplitJob(
        source_name="Modern Interior Master Tileset 32x32 source part 1.png",
        output_pattern="Modern Interior Master Tileset 32x32 part {part}.png",
        explicit_part_tile_counts=(8512,),
        output_start_index=1,
    ),
    SplitJob(
        source_name="Modern Interior Master Tileset 32x32 source part 2.png",
        output_pattern="Modern Interior Master Tileset 32x32 part {part}.png",
        explicit_part_tile_counts=(8512,),
        output_start_index=2,
    ),
    SplitJob(
        source_name="Modern Exterior Master Tileset 32x32.png",
        output_pattern="Modern Exterior Master Tileset 32x32 part {part}.png",
    ),
]


def split_tileset(root: Path, job: SplitJob) -> None:
    source_path = root / job.source_name
    with Image.open(source_path) as source_image:
        source_width, source_height = source_image.size
        if source_width % TILE_SIZE != 0 or source_height % TILE_SIZE != 0:
            raise ValueError(f"{source_path.name} is not aligned to {TILE_SIZE}x{TILE_SIZE} tiles.")

        source_columns = source_width // TILE_SIZE
        total_tiles = (source_width * source_height) // (TILE_SIZE * TILE_SIZE)
        if job.explicit_part_tile_counts is None:
            part_tile_counts = []
            remaining_tiles = total_tiles
            while remaining_tiles > 0:
                current_count = min(TILES_PER_PART, remaining_tiles)
                part_tile_counts.append(current_count)
                remaining_tiles -= current_count
        else:
            part_tile_counts = list(job.explicit_part_tile_counts)
            if sum(part_tile_counts) != total_tiles:
                raise ValueError(
                    f"{source_path.name} explicit part counts total {sum(part_tile_counts)} tiles, expected {total_tiles}."
                )

        part_count = len(part_tile_counts)

        print(
            f"{source_path.name}: {source_width}x{source_height}, "
            f"{total_tiles} tiles, {part_count} part(s)."
        )

        for part_index in range(part_count):
            tile_start = part_index * TILES_PER_PART
            part_tile_count = part_tile_counts[part_index]
            tile_end = tile_start + part_tile_count
            part_rows = math.ceil(part_tile_count / PART_COLUMNS)

            output_path = root / job.output_pattern.format(part=job.output_start_index + part_index)
            part_image = Image.new(source_image.mode, (PART_COLUMNS * TILE_SIZE, part_rows * TILE_SIZE))

            for local_tile_index in range(part_tile_count):
                global_tile_index = tile_start + local_tile_index
                source_tile_x = (global_tile_index % source_columns) * TILE_SIZE
                source_tile_y = (global_tile_index // source_columns) * TILE_SIZE
                tile = source_image.crop(
                    (
                        source_tile_x,
                        source_tile_y,
                        source_tile_x + TILE_SIZE,
                        source_tile_y + TILE_SIZE,
                    )
                )

                dest_tile_x = (local_tile_index % PART_COLUMNS) * TILE_SIZE
                dest_tile_y = (local_tile_index // PART_COLUMNS) * TILE_SIZE
                part_image.paste(tile, (dest_tile_x, dest_tile_y))

            part_image.save(output_path)
            print(f"  -> {output_path.name}: {part_image.size[0]}x{part_image.size[1]}, {part_tile_count} tiles")


def main() -> None:
    root = Path(__file__).resolve().parent
    for job in JOBS:
        split_tileset(root, job)


if __name__ == "__main__":
    main()
