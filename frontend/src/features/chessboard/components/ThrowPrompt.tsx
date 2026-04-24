import { useEffect, useEffectEvent, useMemo, useRef, useState } from "react";

import {
    idxToLogicalPoint,
    logicalPoint,
    offset,
    pointDistanceSquared,
    pointToStr,
    sortPointsByDistanceSquared,
} from "@/features/point/pointUtils";

import { PersistentBoardEffectType } from "../stores/boardEffectsSlice";
import { LogicalPoint, Offset, Point } from "@/features/point/types";
import { useChessboardStore } from "../hooks/useChessboard";
import { PendingThrow } from "../stores/throwSlice";
import { GameColor } from "@/lib/apiClient";
import ChessSquare from "./ChessSquare";
import clsx from "clsx";

interface ThrowLane {
    origin: LogicalPoint;
    points: LogicalPoint[];
}

interface ThrowData {
    direction: Offset;
    lanes: ThrowLane[];
}

export const THROW_COMMIT_DELAY_MS = 1000;
export const THROW_INTENT_DELAY_MS = 500;
export const DEFAULT_THROW_STEP_SIZE = 100;

const ThrowPrompt = () => {
    const pendingThrow = useChessboardStore((x) => x.pendingThrow);
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);
    const boardRect = useChessboardStore((x) => x.boardRect);
    const {
        viewingFrom,
        addPersistentBoardEffect,
        removePersistentBoardEffect,
    } = useChessboardStore((x) => ({
        viewingFrom: x.viewingFrom,
        addPersistentBoardEffect: x.addPersistentBoardEffect,
        removePersistentBoardEffect: x.removePersistentBoardEffect,
    }));

    const [selectedSideIdx, setSelectedSide] = useState<number>(0);
    const [selectedPointIdx, setSelectedIdx] = useState<number>(0);
    const [isHolding, setIsHolding] = useState<boolean>(false);

    const throwData = useMemo(() => getThrowData(pendingThrow), [pendingThrow]);

    const selectedSideRef = useRef(selectedSideIdx);
    const selectedPointRef = useRef(selectedPointIdx);

    useEffect(() => {
        selectedSideRef.current = selectedSideIdx;
    }, [selectedSideIdx]);

    useEffect(() => {
        selectedPointRef.current = selectedPointIdx;
    }, [selectedPointIdx]);

    function updateThrowLine({
        newSelectedSideIdx,
        newSelectedPointIdx,
    }:
        | {
              newSelectedSideIdx: number;
              newSelectedPointIdx?: number;
          }
        | {
              newSelectedSideIdx?: number;
              newSelectedPointIdx: number;
          }) {
        if (!pendingThrow || !throwData) {
            return;
        }

        newSelectedSideIdx ??= selectedSideRef.current;
        newSelectedSideIdx = clampSideIdx(newSelectedSideIdx, throwData);
        setSelectedSide(newSelectedSideIdx);

        if (throwData.lanes.length === 0) {
            return;
        }
        const newLane = throwData.lanes[newSelectedSideIdx];

        newSelectedPointIdx ??= selectedPointRef.current;
        if (
            newSelectedPointIdx === selectedPointRef.current &&
            newSelectedSideIdx !== selectedSideRef.current
        ) {
            const prevLane = throwData.lanes[selectedSideRef.current];
            const prevPoint = prevLane.points[selectedPointRef.current];
            let bestIdx = 0;
            let bestDist = Infinity;

            for (let i = 0; i < newLane.points.length; i++) {
                const newPoint = newLane.points[i];
                const distance = pointDistanceSquared(prevPoint, newPoint);

                if (distance < bestDist) {
                    bestDist = distance;
                    bestIdx = i;
                }
            }
            newSelectedPointIdx = bestIdx;
        }

        newSelectedPointIdx = clampPointIdx(newSelectedPointIdx, newLane);
        setSelectedIdx(newSelectedPointIdx);
    }

    const fixSelectedSideEffectEvent = useEffectEvent(
        (pendingThrow: PendingThrow | null, throwData: ThrowData | null) => {
            if (!pendingThrow || !throwData) {
                return;
            }

            const lane =
                throwData.lanes[clampSideIdx(selectedSideIdx, throwData)];
            updateThrowLine({
                newSelectedPointIdx: Math.floor((lane.points.length - 1) / 2),
            });
        },
    );
    useEffect(() => {
        fixSelectedSideEffectEvent(pendingThrow, throwData);
    }, [pendingThrow, throwData]);

    useEffect(() => {
        if (!pendingThrow || !throwData) {
            return;
        }

        const lane = throwData.lanes[selectedSideIdx];
        if (!lane) {
            return;
        }

        const to = lane.points[selectedPointIdx];
        if (!to) {
            return;
        }

        const effectId = addPersistentBoardEffect({
            type: PersistentBoardEffectType.THROW_AIM_LINE,
            from: pendingThrow.piece.position,
            mid: lane.points[0],
            to,
        });

        return () => {
            removePersistentBoardEffect(effectId);
        };
    }, [
        pendingThrow,
        throwData,
        selectedPointIdx,
        selectedSideIdx,
        removePersistentBoardEffect,
        addPersistentBoardEffect,
    ]);

    function handlePointerDown(event: React.PointerEvent): void {
        if (!pendingThrow || !throwData) {
            return;
        }
        event.stopPropagation();

        if (
            event.button !== 0 ||
            (event.pointerType === "touch" && !event.isPrimary)
        ) {
            pendingThrow.resolve(null);
            return;
        }

        let stepSize = DEFAULT_THROW_STEP_SIZE;
        if (boardRect) {
            stepSize = (boardRect.height / boardDimensions.height) * 1.5;
        }

        const startY = event.clientY;
        const startX = event.clientX;
        const viewForwardY = viewingFrom === GameColor.WHITE ? 1 : -1;

        let animationFrameId: number | null = null;

        let lastPointSnap: Point = { x: startX, y: startY };
        let lastPointIdx = selectedPointIdx;
        let lastSideSnap: Point = { x: startX, y: startY };
        let lastSideIdx = selectedSideIdx;

        let lastY = event.clientY;
        let lastX = event.clientX;
        let didMove = false;

        const intentTimeout = setTimeout(() => {
            if (!didMove) {
                setIsHolding(true);
            }
        }, THROW_INTENT_DELAY_MS);
        const commitTimeout = setTimeout(() => {
            if (didMove) {
                return;
            }

            pendingThrow.resolve(
                throwData.lanes[selectedSideIdx]?.points[selectedPointIdx],
            );
            cleanup();
        }, THROW_INTENT_DELAY_MS + THROW_COMMIT_DELAY_MS);

        function applyIndexDelta() {
            if (!pendingThrow || !throwData) {
                return;
            }

            const dirX = throwData.direction.x;
            const dirY = -throwData.direction.y;

            const sideXDir = -dirY;
            const sideYDir = dirX;
            const sideXDistance = lastX - lastSideSnap.x;
            const sideYDistance = lastY - lastSideSnap.y;
            const sideLen = Math.hypot(sideXDir, sideYDir);
            const sideDistance =
                (sideXDistance * sideXDir + sideYDistance * sideYDir) / sideLen;
            const sideIdxChange =
                Math.trunc(sideDistance / stepSize) * viewForwardY;

            const pointXDir = dirX;
            const pointYDir = dirY;
            const pointXDistance = lastX - lastPointSnap.x;
            const pointYDistance = lastY - lastPointSnap.y;
            const pointLen = Math.hypot(pointXDir, pointYDir);
            const pointDistance =
                (pointXDistance * pointXDir + pointYDistance * pointYDir) /
                pointLen;
            const pointIdxChange =
                Math.trunc(pointDistance / stepSize) * viewForwardY;

            if (sideIdxChange === 0 && pointIdxChange === 0) {
                animationFrameId = null;
                return;
            }

            if (sideIdxChange !== 0) {
                lastSideSnap = { x: lastX, y: lastY };
            }
            if (pointIdxChange !== 0) {
                lastPointSnap = { x: lastX, y: lastY };
            }

            lastPointIdx += pointIdxChange;
            lastSideIdx += sideIdxChange;
            updateThrowLine({
                newSelectedSideIdx: lastSideIdx,
                newSelectedPointIdx: lastPointIdx,
            });
            animationFrameId = null;
            didMove = true;
            setIsHolding(false);
        }

        function handlePointerMove(event: PointerEvent) {
            lastY = event.clientY;
            lastX = event.clientX;
            if (animationFrameId === null) {
                animationFrameId = requestAnimationFrame(applyIndexDelta);
            }
        }

        function handlePointerUp(event: PointerEvent) {
            cleanup();

            if (!pendingThrow || !throwData || didMove) {
                return null;
            }

            event.stopPropagation();
            updateThrowLine({
                newSelectedSideIdx:
                    (selectedSideIdx + 1) % throwData.lanes.length,
                newSelectedPointIdx: selectedPointIdx,
            });
        }

        function cleanup() {
            setIsHolding(false);
            clearTimeout(intentTimeout);
            clearTimeout(commitTimeout);

            document.removeEventListener("pointermove", handlePointerMove);
            document.removeEventListener("pointerup", handlePointerUp);
        }

        document.addEventListener("pointermove", handlePointerMove);
        document.addEventListener("pointerup", handlePointerUp);
    }

    function handleWheel(event: React.WheelEvent) {
        if (!pendingThrow || !throwData) {
            return;
        }
        event.stopPropagation();

        const viewForwardY = viewingFrom === pendingThrow.piece.color ? 1 : -1;
        if (event.deltaY > 0) {
            updateThrowLine({
                newSelectedPointIdx: selectedPointIdx - viewForwardY,
            });
        } else {
            updateThrowLine({
                newSelectedPointIdx: selectedPointIdx + viewForwardY,
            });
        }
    }

    if (!pendingThrow || !throwData) {
        return null;
    }

    const selectedPoint: LogicalPoint | undefined =
        throwData.lanes[selectedSideIdx]?.points[selectedPointIdx];
    if (!selectedPoint) {
        return null;
    }

    const pointsSet = new Set(pendingThrow.points.map(pointToStr));
    return (
        <div
            data-testid="throwPromptOverlay"
            className="absolute inset-0 z-35"
            onPointerDown={handlePointerDown}
            onWheel={handleWheel}
        >
            {[
                ...Array(
                    boardDimensions.height * boardDimensions.height,
                ).keys(),
            ].map((i) => {
                const position = idxToLogicalPoint(i, boardDimensions.height);
                if (pointsSet.has(pointToStr(position))) {
                    return null;
                }

                return (
                    <ChessSquare
                        key={i}
                        position={position}
                        className="bg-black/50"
                        data-testid="throwPromptOverlaySquare"
                    />
                );
            })}

            {pendingThrow.points.map(
                (point, i) =>
                    isOnThrowLine(point, selectedSideIdx, throwData) && (
                        <ChessSquare
                            key={i}
                            position={point}
                            className="bg-secondary/50"
                            data-testid="throwPromptSelectedLineSquare"
                        />
                    ),
            )}

            <ChessSquare
                position={selectedPoint}
                data-testid="throwPromptSelectedSquare"
                className={clsx(
                    "bg-red-500",
                    isHolding && "animate-fast-blink transition-opacity",
                )}
            />
        </div>
    );
};
export default ThrowPrompt;

function isOnThrowLine(
    point: LogicalPoint,
    sideIdx: number,
    throwData: ThrowData,
): boolean {
    const origin = throwData.lanes[sideIdx]?.origin;
    if (!origin) {
        return false;
    }
    return isCollinearWithDirection(point, origin, throwData.direction);
}

function isCollinearWithDirection(
    point: LogicalPoint,
    origin: LogicalPoint,
    direction: Offset,
) {
    return (
        (point.x - origin.x) * direction.y ===
        (point.y - origin.y) * direction.x
    );
}

function clampPointIdx(pointIdx: number, throwLane: ThrowLane): number {
    return Math.max(Math.min(pointIdx, throwLane.points.length - 1), 0);
}

function clampSideIdx(sideIdx: number, throwData: ThrowData): number {
    return Math.max(Math.min(sideIdx, throwData.lanes.length - 1), 0);
}

function buildThrowData(
    pendingThrow: PendingThrow,
    direction: Offset,
    {
        leftOrigin,
        centerOrigin,
        rightOrigin,
    }: {
        leftOrigin: LogicalPoint;
        centerOrigin: LogicalPoint;
        rightOrigin: LogicalPoint;
    },
): ThrowData {
    const leftPoints: LogicalPoint[] = [];
    const centerPoints: LogicalPoint[] = [];
    const rightPoints: LogicalPoint[] = [];
    for (const point of pendingThrow.points) {
        if (isCollinearWithDirection(point, leftOrigin, direction)) {
            leftPoints.push(point);
        } else if (isCollinearWithDirection(point, centerOrigin, direction)) {
            centerPoints.push(point);
        } else if (isCollinearWithDirection(point, rightOrigin, direction)) {
            rightPoints.push(point);
        }
    }

    const lanes: ThrowLane[] = [];
    if (leftPoints.length > 0) {
        lanes.push({
            origin: leftOrigin,
            points: sortPointsByDistanceSquared(leftOrigin, leftPoints),
        });
    }
    if (centerPoints.length > 0) {
        lanes.push({
            origin: centerOrigin,
            points: sortPointsByDistanceSquared(centerOrigin, centerPoints),
        });
    }
    if (rightPoints.length > 0) {
        lanes.push({
            origin: rightOrigin,
            points: sortPointsByDistanceSquared(rightOrigin, rightPoints),
        });
    }

    // this needs to be ordered from the left, from the player non flipped perspective
    const orderedLanes =
        pendingThrow.piece.color === GameColor.WHITE ? lanes : lanes.reverse();

    return {
        direction,
        lanes: orderedLanes,
    };
}

function getThrowData(pendingThrow: PendingThrow | null): ThrowData | null {
    if (pendingThrow === null) {
        return null;
    }

    const forwardY = pendingThrow.piece.color === GameColor.WHITE ? 1 : -1;

    if (pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x === 0) {
        // forward
        return buildThrowData(pendingThrow, offset({ x: 0, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x - 1,
                y: pendingThrow.piece.position.y,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x + 1,
                y: pendingThrow.piece.position.y,
            }),
        });
    } else if (
        pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x >
        0
    ) {
        // left
        return buildThrowData(pendingThrow, offset({ x: -1, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
        });
    } else {
        // right
        return buildThrowData(pendingThrow, offset({ x: 1, y: forwardY }), {
            leftOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
            centerOrigin: pendingThrow.piece.position,
            rightOrigin: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
        });
    }
}
