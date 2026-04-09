import { useEffect, useEffectEvent, useMemo, useState } from "react";

import {
    idxToLogicalPoint,
    logicalPoint,
    offset,
    pointDistanceSquared,
    pointToStr,
} from "@/features/point/pointUtils";

import { PersistentBoardEffectType } from "../stores/boardEffectsSlice";
import { LogicalPoint, Offset } from "@/features/point/types";
import { useChessboardStore } from "../hooks/useChessboard";
import { PendingThrow } from "../stores/throwSlice";
import { GameColor } from "@/lib/apiClient";
import ChessSquare from "./ChessSquare";

export enum ThrowSide {
    LEFT,
    CENTER,
    RIGHT,
}

interface ThrowData {
    direction: Offset;
    [ThrowSide.LEFT]: LogicalPoint;
    [ThrowSide.CENTER]: LogicalPoint;
    [ThrowSide.RIGHT]: LogicalPoint;
}

export const INITIAL_CHARGE_DELAY_MS = 500;

export const CHARGE_STEP_MIN_DELAY_MS = 50;
export const CHARGE_STEP_MAX_DELAY_MS = 250;

export const CHARGE_OSCILLATION_STEP_DELAY_MS = 100;
export const CHARGE_OSCILLATION_UPPER_INDEX = 1;
export const CHARGE_OSCILLATION_LOWER_INDEX = 3;

const ThrowPrompt = () => {
    const pendingThrow = useChessboardStore((x) => x.pendingThrow);
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);
    const { addPersistentBoardEffect, removePersistentBoardEffect } =
        useChessboardStore((x) => ({
            addPersistentBoardEffect: x.addPersistentBoardEffect,
            removePersistentBoardEffect: x.removePersistentBoardEffect,
        }));

    const [selectedSide, setSelectedSide] = useState<ThrowSide>(
        ThrowSide.CENTER,
    );
    const [selectedPoint, setSelectedPoint] = useState<LogicalPoint | null>(
        null,
    );

    const throwData = useMemo(() => getThrowData(pendingThrow), [pendingThrow]);

    useEffect(() => {
        if (!pendingThrow || !throwData) {
            return;
        }

        const startPoint = throwData[selectedSide];

        let min: LogicalPoint | null = null;
        let minDistance = Infinity;

        let max: LogicalPoint | null = null;
        let maxDistance = 0;

        for (const point of pendingThrow.points) {
            if (!isOnLine(point, selectedSide, throwData)) {
                continue;
            }

            const distance = pointDistanceSquared(startPoint, point);
            if (distance > maxDistance) {
                max = point;
                maxDistance = distance;
            }
            if (distance < minDistance) {
                min = point;
                minDistance = distance;
            }
        }
        if (min === null || max === null) {
            return;
        }

        const effectId = addPersistentBoardEffect({
            type: PersistentBoardEffectType.THROW_AIM_LINE,
            from: pendingThrow.piece.position,
            mid: min,
            to: max,
        });

        return () => {
            removePersistentBoardEffect(effectId);
        };
    }, [
        pendingThrow,
        throwData,
        selectedSide,
        addPersistentBoardEffect,
        removePersistentBoardEffect,
    ]);

    const fixSelectedSideEffectEvent = useEffectEvent(
        (pendingThrow: PendingThrow | null, throwData: ThrowData | null) => {
            if (!pendingThrow || !throwData) {
                return;
            }

            const availableSides = getAvailableSides(pendingThrow, throwData);
            if (!availableSides.some((x) => x === selectedSide)) {
                setSelectedSide(availableSides[0]);
            }
        },
    );
    useEffect(() => {
        fixSelectedSideEffectEvent(pendingThrow, throwData);
    }, [pendingThrow, throwData]);

    function getNextSide(
        current: ThrowSide,
        pendingThrow: PendingThrow,
        throwData: ThrowData,
    ) {
        const availableSides = getAvailableSides(pendingThrow, throwData);
        if (availableSides.length === 0) {
            return current;
        }

        const currentIdx = availableSides.indexOf(current);
        const nextIdx = (currentIdx + 1) % availableSides.length;
        return availableSides[nextIdx];
    }

    function handlePress(event: React.PointerEvent): void {
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

        const startPoint = throwData[selectedSide];
        const pointsFromClosest = pendingThrow.points
            .filter((x) => isOnLine(x, selectedSide, throwData))
            .sort((a, b) => {
                const distanceA = pointDistanceSquared(startPoint, a);
                const distanceB = pointDistanceSquared(startPoint, b);
                return distanceA - distanceB;
            });
        const numOfSteps = pointsFromClosest.length;

        let targetIdx = -1;
        let reachedMaxPower = false;
        let direction = 1;
        const maxPowerLowerBound = Math.max(
            0,
            numOfSteps - 1 - CHARGE_OSCILLATION_LOWER_INDEX,
        );
        const maxPowerUpperBound = Math.max(
            0,
            numOfSteps - 1 - CHARGE_OSCILLATION_UPPER_INDEX,
        );

        let timeout = setTimeout(animateThrowPower, INITIAL_CHARGE_DELAY_MS);
        function animateThrowPower() {
            if (
                reachedMaxPower &&
                direction === 1 &&
                targetIdx >= maxPowerUpperBound
            ) {
                direction = -1;
            } else if (
                reachedMaxPower &&
                direction === -1 &&
                targetIdx <= maxPowerLowerBound
            ) {
                direction = 1;
            }

            targetIdx = Math.max(
                0,
                Math.min(numOfSteps - 1, targetIdx + direction),
            );
            setSelectedPoint(pointsFromClosest[targetIdx]);

            if (targetIdx >= numOfSteps - 1) {
                reachedMaxPower = true;
            }

            if (reachedMaxPower) {
                timeout = setTimeout(
                    animateThrowPower,
                    CHARGE_OSCILLATION_STEP_DELAY_MS,
                );
                return;
            }

            const time = targetIdx / (boardDimensions.height - 1);
            const delay =
                CHARGE_STEP_MIN_DELAY_MS +
                (CHARGE_STEP_MAX_DELAY_MS - CHARGE_STEP_MIN_DELAY_MS) *
                    (1 - time) ** 2;
            timeout = setTimeout(animateThrowPower, delay);
        }

        function onPointerUp() {
            document.removeEventListener("pointerup", onPointerUp);

            clearTimeout(timeout);
            setSelectedPoint(null);

            if (!pendingThrow || !throwData) {
                return;
            }

            if (targetIdx !== -1) {
                pendingThrow?.resolve(pointsFromClosest[targetIdx]);
                return;
            }

            setSelectedSide(getNextSide(selectedSide, pendingThrow, throwData));
        }

        document.addEventListener("pointerup", onPointerUp);
    }

    if (!pendingThrow || !throwData) {
        return null;
    }
    const pointsSet = new Set(pendingThrow.points.map(pointToStr));
    return (
        <div
            data-testid="throwPromptOverlay"
            className="absolute inset-0 z-35"
            onPointerDown={handlePress}
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
                    isOnLine(point, selectedSide, throwData) && (
                        <ChessSquare
                            key={i}
                            position={point}
                            className="bg-secondary/50"
                            data-testid="throwPromptSelectedLineSquare"
                        />
                    ),
            )}

            {selectedPoint && (
                <ChessSquare
                    position={selectedPoint}
                    className="bg-red-500"
                    data-testid="throwPromptSelectedSquare"
                />
            )}
        </div>
    );
};
export default ThrowPrompt;

function isOnLine(
    point: LogicalPoint,
    side: ThrowSide,
    throwData: ThrowData,
): boolean {
    const origin = throwData[side];
    return (
        (point.x - origin.x) * throwData.direction.y ===
        (point.y - origin.y) * throwData.direction.x
    );
}

function getAvailableSides(
    pendingThrow: PendingThrow,
    throwData: ThrowData,
): ThrowSide[] {
    const sides: ThrowSide[] = [
        ThrowSide.LEFT,
        ThrowSide.CENTER,
        ThrowSide.RIGHT,
    ];
    const availableSides = sides.filter((side) =>
        pendingThrow.points.some((x) => isOnLine(x, side, throwData)),
    );
    return availableSides;
}

function getThrowData(pendingThrow: PendingThrow | null): ThrowData | null {
    if (pendingThrow === null) {
        return null;
    }

    const forwardY = pendingThrow.piece.color === GameColor.WHITE ? 1 : -1;

    if (pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x === 0) {
        // forward
        return {
            direction: offset({ x: 0, y: forwardY }),
            [ThrowSide.LEFT]: logicalPoint({
                x: pendingThrow.piece.position.x - 1,
                y: pendingThrow.piece.position.y,
            }),
            [ThrowSide.CENTER]: pendingThrow.piece.position,
            [ThrowSide.RIGHT]: logicalPoint({
                x: pendingThrow.piece.position.x + 1,
                y: pendingThrow.piece.position.y,
            }),
        };
    } else if (
        pendingThrow.throwerOrigin.x - pendingThrow.piece.position.x >
        0
    ) {
        // left
        return {
            direction: offset({ x: -1, y: forwardY }),
            [ThrowSide.LEFT]: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
            [ThrowSide.CENTER]: pendingThrow.piece.position,
            [ThrowSide.RIGHT]: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
        };
    } else {
        // right
        return {
            direction: offset({ x: 1, y: forwardY }),
            [ThrowSide.LEFT]: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y + forwardY,
            }),
            [ThrowSide.CENTER]: pendingThrow.piece.position,
            [ThrowSide.RIGHT]: logicalPoint({
                x: pendingThrow.piece.position.x,
                y: pendingThrow.piece.position.y - forwardY,
            }),
        };
    }
}
