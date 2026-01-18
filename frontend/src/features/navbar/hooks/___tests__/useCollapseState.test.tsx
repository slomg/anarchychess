import { renderHook, act } from "@testing-library/react";

import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockDom";
import useCollapseState from "../useCollapseState";
import constants from "@/lib/constants";

describe("useCollapseState", () => {
    it("should default to not collapsed when there is no localstorage state", () => {
        const { result } = renderHook(() => useCollapseState());

        expect(result.current.isCollapsed).toBe(false);
    });

    it("should load initial value from localStorage", () => {
        localStorage.setItem(
            constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED,
            "true",
        );

        const { result } = renderHook(() => useCollapseState());

        expect(result.current.isCollapsed).toBe(true);
    });

    it("should set isCollapsed true on small screens", () => {
        setWindowInnerWidth(800);

        const { result } = renderHook(() => useCollapseState());
        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(true);
    });

    it("should use localstorage value on larger screens", () => {
        localStorage.setItem(
            constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED,
            "true",
        );

        const { result } = renderHook(() => useCollapseState());
        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(true);
    });

    it("should preserve localStorage value when resizing from small to large screen", () => {
        localStorage.setItem(
            constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED,
            "false",
        );

        setWindowInnerWidth(800);

        const { result } = renderHook(() => useCollapseState());

        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(true);
        expect(
            localStorage.getItem(constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED),
        ).toBe("false");

        setWindowInnerWidth(1200);
        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(false);
        expect(
            localStorage.getItem(constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED),
        ).toBe("false");
    });

    it("should toggle state and sets/removes cookie", () => {
        const { result } = renderHook(() => useCollapseState());

        // collapse
        act(() => {
            result.current.toggleCollapse();
        });
        expect(result.current.isCollapsed).toBe(true);
        expect(
            localStorage.getItem(constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED),
        ).toBe("true");

        // expand
        act(() => {
            result.current.toggleCollapse();
        });
        expect(result.current.isCollapsed).toBe(false);
        expect(
            localStorage.getItem(constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED),
        ).toBe("false");
    });
});
