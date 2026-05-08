import { useEffect, useReducer, useState } from "react";
import clsx from "clsx";

import {
    idxToLogicalPoint,
    pointDistanceSquared,
    pointToStr,
} from "@/features/point/pointUtils";
import {
    clampPointIdx,
    clampLaneIdx,
    getThrowData,
    isOnLane,
    ThrowData,
} from "../lib/throwUtils";

import { PersistentBoardEffectType } from "../stores/boardEffectsSlice";
import { LogicalPoint, Point } from "@/features/point/types";
import { useChessboardStore } from "../hooks/useChessboard";
import { GameColor } from "@/lib/apiClient";
import ChessSquare from "./ChessSquare";
import constants from "@/lib/constants";

export const THROW_COMMIT_DELAY_MS = 1000;
export const THROW_INTENT_DELAY_MS = 500;
export const DEFAULT_THROW_STEP_SIZE = 100;

type ThrowPromptState =
    | {
          selectedLaneIdx: null;
          selectedPointIdx: null;
          throwData: null;
      }
    | {
          selectedLaneIdx: number;
          selectedPointIdx: number;
          throwData: ThrowData;
      };

type ThrowPromptAction =
    | {
          type: "move";
          newSelectedLaneIdx: number;
          newSelectedPointIdx?: number;
      }
    | {
          type: "move";
          newSelectedLaneIdx?: number;
          newSelectedPointIdx: number;
      }
    | { type: "cycle" }
    | { type: "updateThrowData"; throwData: ThrowData | null };

function reducer(
    state: ThrowPromptState,
    action: ThrowPromptAction,
): ThrowPromptState {
    switch (action.type) {
        case "move":
            if (state.throwData === null) {
                return state;
            }

            let newLaneIdx = action.newSelectedLaneIdx ?? state.selectedLaneIdx;
            newLaneIdx = clampLaneIdx(newLaneIdx, state.throwData);

            const newLane = state.throwData.lanes[newLaneIdx];
            const newPointIdx = clampPointIdx(
                action.newSelectedPointIdx ?? state.selectedPointIdx,
                newLane,
            );
            if (
                newPointIdx !== state.selectedPointIdx ||
                newLaneIdx === state.selectedLaneIdx
            ) {
                return {
                    ...state,
                    selectedLaneIdx: newLaneIdx,
                    selectedPointIdx: newPointIdx,
                };
            }

            const prevLane = state.throwData.lanes[state.selectedLaneIdx];
            const prevPoint = prevLane.points[state.selectedPointIdx];

            let closestPointIdx = 0;
            let closestDist = Infinity;
            for (let i = 0; i < newLane.points.length; i++) {
                const newPoint = newLane.points[i];
                const dist = pointDistanceSquared(prevPoint, newPoint);

                if (
                    dist < closestDist ||
                    (dist === closestDist &&
                        Math.abs(i - state.selectedPointIdx) <
                            Math.abs(closestPointIdx - state.selectedPointIdx))
                ) {
                    closestDist = dist;
                    closestPointIdx = i;
                }
            }
            return {
                ...state,
                selectedLaneIdx: newLaneIdx,
                selectedPointIdx: closestPointIdx,
            };

        case "cycle":
            if (state.throwData === null) {
                return state;
            }

            const newSelectedLaneIdx =
                (state.selectedLaneIdx + 1) % state.throwData.lanes.length;
            return reducer(state, {
                type: "move",
                newSelectedLaneIdx,
            });

        case "updateThrowData":
            if (action.throwData === null) {
                return {
                    selectedLaneIdx: null,
                    selectedPointIdx: null,
                    throwData: null,
                };
            }

            const newState: ThrowPromptState = {
                throwData: action.throwData,
                selectedLaneIdx: 0,
                selectedPointIdx: Math.floor(
                    (action.throwData.lanes[0].points.length - 1) / 2,
                ),
            };

            return reducer(newState, {
                type: "move",
                newSelectedPointIdx: newState.selectedPointIdx,
            });

        default:
            return state;
    }
}

const ThrowPrompt = () => {
    const pendingThrow = useChessboardStore((x) => x.pendingThrow);
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

    const [isHolding, setIsHolding] = useState<boolean>(false);
    const [state, dispatch] = useReducer(reducer, {
        selectedLaneIdx: null,
        selectedPointIdx: null,
        throwData: null,
    });

    useEffect(() => {
        const throwData = getThrowData(pendingThrow);
        dispatch({ type: "updateThrowData", throwData });
    }, [pendingThrow]);

    useEffect(() => {
        if (!pendingThrow || !state.throwData) {
            return;
        }

        const lane = state.throwData.lanes[state.selectedLaneIdx];
        const to = lane.points[state.selectedPointIdx];
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
        state.throwData,
        state.selectedLaneIdx,
        state.selectedPointIdx,
        removePersistentBoardEffect,
        addPersistentBoardEffect,
    ]);

    function handlePointerDown(event: React.PointerEvent): void {
        if (!pendingThrow || !state.throwData) {
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
            stepSize = (boardRect.height / constants.BOARD_HEIGHT) * 1.5;
        }

        const startY = event.clientY;
        const startX = event.clientX;
        const viewForwardY = viewingFrom === GameColor.WHITE ? 1 : -1;

        let animationFrameId: number | null = null;

        let lastPointSnap: Point = { x: startX, y: startY };
        let lastPointIdx = state.selectedPointIdx;
        let lastLaneSnap: Point = { x: startX, y: startY };
        let lastLaneIdx = state.selectedLaneIdx;

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
                state.throwData.lanes[state.selectedLaneIdx].points[
                    state.selectedPointIdx
                ],
            );
            cleanup();
        }, THROW_INTENT_DELAY_MS + THROW_COMMIT_DELAY_MS);

        function applyIndexDelta() {
            if (!pendingThrow || !state.throwData) {
                return;
            }

            const dirX = state.throwData.direction.x;
            const dirY = -state.throwData.direction.y;

            const laneXDir = -dirY;
            const laneYDir = dirX;
            const laneXDistance = lastX - lastLaneSnap.x;
            const laneYDistance = lastY - lastLaneSnap.y;
            const laneLen = Math.hypot(laneXDir, laneYDir);
            const laneDistance =
                (laneXDistance * laneXDir + laneYDistance * laneYDir) / laneLen;
            const laneIdxChange =
                Math.trunc(laneDistance / stepSize) * viewForwardY;

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

            if (laneIdxChange === 0 && pointIdxChange === 0) {
                animationFrameId = null;
                return;
            }

            if (laneIdxChange !== 0) {
                lastLaneSnap = { x: lastX, y: lastY };
            }
            if (pointIdxChange !== 0) {
                lastPointSnap = { x: lastX, y: lastY };
            }

            lastPointIdx += pointIdxChange;
            lastLaneIdx += laneIdxChange;
            dispatch({
                type: "move",
                newSelectedLaneIdx: lastLaneIdx,
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
            event.stopPropagation();

            if (!didMove) {
                dispatch({ type: "cycle" });
            }
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
        if (!pendingThrow || !state.throwData) {
            return;
        }
        event.stopPropagation();

        const viewForwardY = viewingFrom === pendingThrow.piece.color ? 1 : -1;
        if (event.deltaY > 0) {
            dispatch({
                type: "move",
                newSelectedPointIdx: state.selectedPointIdx - viewForwardY,
            });
        } else {
            dispatch({
                type: "move",
                newSelectedPointIdx: state.selectedPointIdx + viewForwardY,
            });
        }
    }

    if (!pendingThrow || !state.throwData) {
        return null;
    }

    const selectedPoint: LogicalPoint =
        state.throwData.lanes[state.selectedLaneIdx].points[
            state.selectedPointIdx
        ];

    const pointsSet = new Set(pendingThrow.points.map(pointToStr));
    return (
        <div
            data-testid="throwPromptOverlay"
            className="absolute inset-0 z-35"
            onPointerDown={handlePointerDown}
            onWheel={handleWheel}
        >
            {[
                ...Array(constants.BOARD_HEIGHT * constants.BOARD_WIDTH).keys(),
            ].map((i) => {
                const position = idxToLogicalPoint(i);
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
                    isOnLane(point, state.selectedLaneIdx, state.throwData) && (
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
