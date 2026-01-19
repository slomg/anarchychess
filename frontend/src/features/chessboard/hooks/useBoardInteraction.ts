import { useCallback, useEffect, useRef, useState } from "react";
import { InteractionInfo } from "../stores/interactionSlice";
import { useChessboardStore } from "./useChessboard";
import { screenPoint } from "@/features/point/pointUtils";
import { ScreenPoint } from "@/features/point/types";
import { MaybePromise } from "@/types/types";

export default function useBoardInteraction({
    shouldStartDrag,

    onDragStart,
    onDragMove,
    onDragEnd,

    onPress,
}: {
    shouldStartDrag: (info: InteractionInfo) => MaybePromise<boolean>;

    onDragStart?: (point: ScreenPoint) => MaybePromise<void>;
    onDragMove?: (point: ScreenPoint) => MaybePromise<void>;
    onDragEnd?: (point: ScreenPoint) => MaybePromise<void>;

    onPress?: (info: InteractionInfo) => MaybePromise<void>;
}): boolean {
    const callbacksRef = useRef({
        shouldStartDrag,
        onDragStart,
        onDragMove,
        onDragEnd,
        onPress,
    });

    useEffect(() => {
        callbacksRef.current = {
            shouldStartDrag,
            onDragStart,
            onDragMove,
            onDragEnd,
            onPress,
        };
    }, [shouldStartDrag, onDragStart, onDragMove, onDragEnd, onPress]);

    const [isDragging, setIsDragging] = useState(false);

    const isDraggingRef = useRef(false);
    const isPointerDown = useRef(false);

    const { pointerDownEvent, dragStartQuery, pointerUpEvent } =
        useChessboardStore((x) => ({
            pointerDownEvent: x.pointerDownEvent,
            dragStartQuery: x.dragStartQuery,
            pointerUpEvent: x.pointerUpEvent,
        }));

    const startDragging = useCallback(async (startFrom: ScreenPoint) => {
        if (!isPointerDown.current) return;

        let animationFrameId: number | null = null;
        let lastMouseX = startFrom.x;
        let lastMouseY = startFrom.y;

        async function emitDrag(): Promise<void> {
            if (!isDraggingRef.current) return;

            await callbacksRef.current.onDragMove?.(
                screenPoint({
                    x: lastMouseX,
                    y: lastMouseY,
                }),
            );
            animationFrameId = null;
        }

        function handleMove(event: PointerEvent) {
            lastMouseX = event.clientX;
            lastMouseY = event.clientY;

            if (animationFrameId === null) {
                animationFrameId = requestAnimationFrame(emitDrag);
            }
        }

        async function stopDragging(event: PointerEvent) {
            if (!isDraggingRef.current) return;
            isDraggingRef.current = false;

            await callbacksRef.current.onDragEnd?.(
                screenPoint({
                    x: event.clientX,
                    y: event.clientY,
                }),
            );
            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);

            setIsDragging(false);
        }

        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);
        await callbacksRef.current.onDragStart?.(startFrom);
        isDraggingRef.current = true;
        setIsDragging(true);

        await emitDrag();
    }, []);

    useEffect(() => {
        async function pointerDownHandler(
            info: InteractionInfo,
        ): Promise<void> {
            isPointerDown.current = true;
            await callbacksRef.current.onPress?.(info);
        }

        async function shouldStartDragHandler(
            info: InteractionInfo,
        ): Promise<boolean> {
            const shouldStart =
                await callbacksRef.current?.shouldStartDrag(info);
            if (shouldStart) await startDragging(info.point);

            return shouldStart;
        }

        async function pointerUpHandler(): Promise<void> {
            isPointerDown.current = false;
        }

        pointerDownEvent.subscribe(pointerDownHandler);
        dragStartQuery.subscribe(shouldStartDragHandler);
        pointerUpEvent.subscribe(pointerUpHandler);

        return () => {
            pointerDownEvent.unsubscribe(pointerDownHandler);
            dragStartQuery.unsubscribe(shouldStartDragHandler);
            pointerUpEvent.unsubscribe(pointerUpHandler);
        };
    }, [pointerUpEvent, dragStartQuery, pointerDownEvent, startDragging]);

    return isDragging;
}
