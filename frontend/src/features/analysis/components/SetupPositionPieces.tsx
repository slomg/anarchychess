import { useRef, useState } from "react";
import Image from "next/image";
import clsx from "clsx";

import useBoardInteraction from "@/features/chessboard/hooks/useBoardInteraction";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { getPieceImage } from "@/features/chessboard/lib/pieceImage";
import { screenPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";

const PIECES = [
    PieceType.KING,
    PieceType.QUEEN,
    PieceType.ROOK,
    PieceType.BISHOP,
    PieceType.HORSEY,
    PieceType.KNOOK,
    PieceType.ANTIQUEEN,
    PieceType.CHECKER,
    PieceType.PAWN,
    PieceType.STERILE_PAWN,
    PieceType.UNDERAGE_PAWN,
];
const NEUTRAL_PIECES = [PieceType.TRAITOR_ROOK];

const SetupPositionPieces = () => {
    const addSetupModePiece = useChessboardStore((x) => x.addSetupModePiece);

    const [selectedPiece, setSelectedPiece] = useState<{
        type: PieceType;
        color: GameColor | null;
    } | null>(null);
    const [draggingPiece, setDraggingPiece] = useState<{
        type: PieceType;
        color: GameColor | null;
    } | null>(null);
    const draggingPieceRef = useRef<HTMLImageElement | null>(null);

    function handlePress(
        event: React.PointerEvent,
        pieceType: PieceType,
        color: GameColor | null,
    ) {
        setDraggingPiece({ type: pieceType, color });

        const startX = event.clientX;
        const startY = event.clientY;

        let lastX = event.clientX;
        let lastY = event.clientY;
        let animationFrameId: number | null = null;

        const DRAG_THRESHOLD = 40;
        let hasDragged = false;

        function handleDrag() {
            if (draggingPieceRef.current === null) {
                return;
            }

            draggingPieceRef.current.style.transform = `translate3d(${lastX - 30}px, ${lastY - 30}px, 0)`;
            draggingPieceRef.current.style.display = "block";

            animationFrameId = null;
        }

        function handlePointerMove(event: PointerEvent) {
            event.preventDefault();
            lastX = event.clientX;
            lastY = event.clientY;
            if (!hasDragged) {
                const dx = lastX - startX;
                const dy = lastY - startY;
                hasDragged = Math.hypot(dx, dy) > DRAG_THRESHOLD;
            }

            if (animationFrameId === null) {
                animationFrameId = requestAnimationFrame(handleDrag);
            }
        }

        function handlePointerUp(event: PointerEvent) {
            document.removeEventListener("pointermove", handlePointerMove);
            document.removeEventListener("pointerup", handlePointerUp);
            setDraggingPiece(null);

            if (hasDragged) {
                addSetupModePiece(
                    pieceType,
                    color,
                    screenPoint({ x: event.clientX, y: event.clientY }),
                );
            } else {
                setSelectedPiece((prev) =>
                    pieceType === prev?.type && color === prev.color
                        ? null
                        : {
                              type: pieceType,
                              color,
                          },
                );
            }
        }

        document.addEventListener("pointermove", handlePointerMove, {
            passive: false,
        });
        document.addEventListener("pointerup", handlePointerUp);
    }

    useBoardInteraction({
        onPress(info) {
            if (selectedPiece === null) {
                return;
            }

            addSetupModePiece(
                selectedPiece.type,
                selectedPiece.color,
                info.point,
            );
        },
    });

    return (
        <div className="grid grid-cols-5">
            {PIECES.map((piece) => (
                <SetupPiece
                    type={piece}
                    color={GameColor.WHITE}
                    isSelected={
                        piece === selectedPiece?.type &&
                        selectedPiece.color === GameColor.WHITE
                    }
                    onPress={(event) =>
                        handlePress(event, piece, GameColor.WHITE)
                    }
                    key={`${piece}-white`}
                />
            ))}

            {PIECES.map((piece) => (
                <SetupPiece
                    type={piece}
                    color={GameColor.BLACK}
                    isSelected={
                        piece === selectedPiece?.type &&
                        selectedPiece.color === GameColor.BLACK
                    }
                    onPress={(event) =>
                        handlePress(event, piece, GameColor.BLACK)
                    }
                    key={`${piece}-black`}
                />
            ))}

            {NEUTRAL_PIECES.map((piece) => (
                <SetupPiece
                    type={piece}
                    color={null}
                    isSelected={
                        piece === selectedPiece?.type &&
                        selectedPiece.color === null
                    }
                    onPress={(event) => handlePress(event, piece, null)}
                    key={`${piece}-neutral`}
                />
            ))}

            {draggingPiece && (
                <Image
                    className="pointer-events-none fixed top-0 left-0 z-50
                        will-change-transform"
                    src={getPieceImage(draggingPiece.type, draggingPiece.color)}
                    ref={draggingPieceRef}
                    width={60}
                    height={60}
                    alt="dragging piece"
                    unoptimized
                    style={{ display: "none" }}
                    data-testid="setupPositionPiecesGhost"
                />
            )}
        </div>
    );
};
export default SetupPositionPieces;

const SetupPiece = ({
    type,
    color,
    isSelected,
    onPress,
}: {
    type: PieceType;
    color: GameColor | null;
    isSelected: boolean;
    onPress: (event: React.PointerEvent) => void;
}) => {
    return (
        <Image
            className={clsx(
                "cursor-pointer touch-none select-none",
                isSelected && "bg-accent rounded-md",
            )}
            src={getPieceImage(type, color)}
            onPointerDown={onPress}
            width={60}
            height={60}
            alt="piece"
            unoptimized
            draggable={false}
            data-testid={`setupPiece-${type}-${color}`}
        />
    );
};
