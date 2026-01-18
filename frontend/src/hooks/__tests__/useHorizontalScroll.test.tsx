import { renderHook } from "@testing-library/react";
import { act } from "react";

import useHorizontalScroll from "../useHorizontalScroll";

describe("useHorizontalScroll", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    function setupElement({
        scrollLeft = 0,
        scrollWidth = 500,
        clientWidth = 100,
        el,
    }: Partial<{
        scrollLeft: number;
        scrollWidth: number;
        clientWidth: number;
        el: HTMLDivElement;
    }> = {}) {
        el ??= document.createElement("div");
        el.scrollLeft = scrollLeft;

        Object.defineProperties(el, {
            scrollWidth: { configurable: true, get: () => scrollWidth },
            clientWidth: { configurable: true, get: () => clientWidth },
        });

        return el;
    }

    it("should scroll right on wheel down", () => {
        const el = setupElement({ scrollLeft: 0 });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const delta = 50;
        const wheelEvent = new WheelEvent("wheel", { deltaY: delta });
        act(() => {
            el.dispatchEvent(wheelEvent);
        });

        vi.runAllTimers();

        expect(el.scrollLeft).toBe(delta);
    });

    it("should scroll left on wheel up", () => {
        const initialScroll = 200;
        const delta = -50;

        const el = setupElement({ scrollLeft: initialScroll });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const wheelEvent = new WheelEvent("wheel", { deltaY: delta });
        act(() => {
            el.dispatchEvent(wheelEvent);
        });

        vi.runAllTimers();

        expect(el.scrollLeft).toBeLessThan(initialScroll + delta);
    });

    it("should not scroll past left boundary", () => {
        const el = setupElement({ scrollLeft: 10 });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const wheelEvent = new WheelEvent("wheel", { deltaY: -100 });
        act(() => {
            el.dispatchEvent(wheelEvent);
        });

        vi.runAllTimers();

        expect(el.scrollLeft).toBe(0);
    });

    it("should not scroll past right boundary", () => {
        const scrollWidth = 500;
        const clientWidth = 100;
        const el = setupElement({
            scrollLeft: scrollWidth - clientWidth - 10,
            scrollWidth,
            clientWidth,
        });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const wheelEvent = new WheelEvent("wheel", { deltaY: 1000 });
        act(() => {
            el.dispatchEvent(wheelEvent);
        });
        vi.runAllTimers();

        expect(el.scrollLeft).toBe(scrollWidth - clientWidth);
    });

    it("should smoothly animate scroll", () => {
        const el = setupElement({ scrollLeft: 0 });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const delta = 100;
        const wheelEvent = new WheelEvent("wheel", { deltaY: delta });
        act(() => {
            el.dispatchEvent(wheelEvent);
        });

        vi.advanceTimersToNextFrame();
        expect(el.scrollLeft).toBeGreaterThan(0);
        expect(el.scrollLeft).toBeLessThan(delta);
        vi.runAllTimers();

        expect(el.scrollLeft).toBe(delta);
    });

    it("should not preventDefault if there is no horizontal scroll", () => {
        const el = setupElement({
            scrollWidth: 100,
            clientWidth: 100,
            scrollLeft: 0,
        });
        const ref = { current: el };

        renderHook(() => useHorizontalScroll(ref));

        const wheelEvent = new WheelEvent("wheel", { deltaY: 50 });
        const preventDefaultSpy = vi.spyOn(wheelEvent, "preventDefault");

        act(() => {
            el.dispatchEvent(wheelEvent);
        });

        vi.runAllTimers();

        expect(preventDefaultSpy).not.toHaveBeenCalled();
    });
});
