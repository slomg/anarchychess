import clsx from "clsx";

import { useChessboardStore } from "../hooks/useChessboard";
import { viewPoint } from "@/features/point/pointUtils";
import { GameColor } from "@/lib/apiClient";
import CoordSquare from "./CoordSquare";
import constants from "@/lib/constants";

const Coords = () => {
    const viewingFrom = useChessboardStore((x) => x.viewingFrom);

    const files = Array.from({ length: constants.BOARD_WIDTH }, (_, i) =>
        String.fromCharCode("a".charCodeAt(0) + i),
    );
    const ranks = Array.from(
        { length: constants.BOARD_HEIGHT },
        (_, i) => constants.BOARD_HEIGHT - i,
    );

    return (
        <>
            {files.map((file, x) => {
                const viewerX = viewingFrom === GameColor.WHITE ? x : 9 - x;
                return (
                    <CoordSquare
                        key={file}
                        data-testid={`coordsFile-${file}`}
                        position={viewPoint({
                            x: viewerX,
                            y: 9,
                        })}
                        className={clsx(
                            "flex items-end px-1 select-none",
                            viewerX % 2 === 0
                                ? "text-[#e9e9d4]"
                                : "text-[#577298]",
                        )}
                    >
                        {file}
                    </CoordSquare>
                );
            })}

            {ranks.map((rank, y) => {
                const viewerY = viewingFrom === GameColor.WHITE ? y : 9 - y;
                return (
                    <CoordSquare
                        key={rank}
                        data-testid={`coordsRank-${rank}`}
                        position={viewPoint({ x: 9, y: viewerY })}
                        className={clsx(
                            "flex justify-end px-1 select-none",
                            viewerY % 2 === 0
                                ? "text-[#e9e9d4]"
                                : "text-[#577298]",
                        )}
                    >
                        {rank}
                    </CoordSquare>
                );
            })}
        </>
    );
};

export default Coords;
