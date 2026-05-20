import WebGL from "three/examples/jsm/capabilities/WebGL.js";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";
import Cookies from "js-cookie";

import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import WebGLWarningAlert from "../WebGLWarningAlert";
import constants from "@/lib/constants";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";

vi.mock("js-cookie");

describe("WebGLWarningAlert", () => {
    let store: StoreApi<ChessboardStore>;

    const cookiesMock = vi.mocked(Cookies);

    beforeEach(() => {
        store = createChessboardStore();

        vi.spyOn(WebGL, "isWebGL2Available").mockReturnValue(false);
        store.setState({ disableDrag: false });
    });

    it("should not show alert if webgl is available", () => {
        vi.spyOn(WebGL, "isWebGL2Available").mockReturnValue(true);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <WebGLWarningAlert />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.queryByTestId("webGlWarningAlert"),
        ).not.toBeInTheDocument();
    });

    it("should not show alert if drag is disabled", () => {
        store.setState({ disableDrag: true });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <WebGLWarningAlert />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.queryByTestId("webGlWarningAlert"),
        ).not.toBeInTheDocument();
    });

    it("should not show alert if dismissed cookie exists", () => {
        mockJsCookie({ [constants.COOKIES.DISMISSED_WEBGL_WARNING]: "true" });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <WebGLWarningAlert />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.queryByTestId("webGlWarningAlert"),
        ).not.toBeInTheDocument();
    });

    it("should show the alert if webgl is not available and drag is disabled", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <WebGLWarningAlert />
            </ChessboardStoreContext.Provider>,
        );

        const alert = screen.getByTestId("webGlWarningAlert");
        expect(alert).toBeInTheDocument();
        expect(alert).toHaveTextContent(
            "WebGL is not supported by your browser, so some animations will not work.",
        );
        expect(
            screen.getByTestId("closeWebGlWarningAlert"),
        ).toBeInTheDocument();
    });

    it("should dismiss alert and store that in a cookie when dismissing", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <WebGLWarningAlert />
            </ChessboardStoreContext.Provider>,
        );

        const alert = screen.getByTestId("webGlWarningAlert");
        expect(alert).toBeInTheDocument();

        await user.click(screen.getByTestId("closeWebGlWarningAlert"));

        expect(alert).not.toBeInTheDocument();
        expect(cookiesMock.set).toHaveBeenCalledExactlyOnceWith(
            constants.COOKIES.DISMISSED_WEBGL_WARNING,
            "true",
        );
    });
});
