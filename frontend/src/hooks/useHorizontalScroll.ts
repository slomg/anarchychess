import { useEffect } from "react";

const SCROLL_SMOOTH_DELTA = 0.2;

export default function useHorizontalScroll(
    ref: React.RefObject<HTMLElement | null>,
) {
    useEffect(() => {
        if (!ref.current) return;

        let animationFrame: number | null = null;
        let targetScroll = 0;
        function animate() {
            if (!ref.current) return;

            const currentScroll = ref.current.scrollLeft;
            const delta = (targetScroll - currentScroll) * SCROLL_SMOOTH_DELTA;

            if (Math.abs(delta) > 0.5) {
                ref.current.scrollLeft = currentScroll + delta;
                animationFrame = requestAnimationFrame(animate);
            } else {
                ref.current.scrollLeft = targetScroll;
                animationFrame = null;
            }
        }

        function onWheel(event: WheelEvent) {
            if (!ref.current) return;
            event.preventDefault();

            targetScroll += event.deltaY;
            targetScroll = Math.max(
                0,
                Math.min(
                    targetScroll,
                    ref.current.scrollWidth - ref.current.clientWidth,
                ),
            );

            if (!animationFrame) {
                animationFrame = requestAnimationFrame(animate);
            }
        }

        const current = ref.current;
        current.addEventListener("wheel", onWheel, { passive: false });
        return () => {
            current.removeEventListener("wheel", onWheel);
        };
    }, [ref]);
}
