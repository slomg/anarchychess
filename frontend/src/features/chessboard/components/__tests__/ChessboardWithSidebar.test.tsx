import { render, screen } from "@testing-library/react";
import ChessboardWithSidebar from "../ChessboardWithSidebar";

describe("ChessboardWithSidebar", () => {
    it("should render chessboard and aside content", () => {
        render(
            <ChessboardWithSidebar
                chessboard={<div data-testid="board" />}
                aside={<div data-testid="sidebar" />}
            />,
        );

        expect(screen.getByTestId("board")).toBeInTheDocument();
        expect(screen.getByTestId("sidebar")).toBeInTheDocument();
    });

    it("should apply flex-col-reverse when prioritizeAside is true", () => {
        render(
            <ChessboardWithSidebar
                chessboard={<div data-testid="board" />}
                aside={<div data-testid="sidebar" />}
                prioritizeAside={true}
            />,
        );

        const main = screen.getByRole("main");
        expect(main).toHaveClass("flex-col-reverse");
        expect(main).not.toHaveClass("flex-col");
    });

    it("should apply flex-col when prioritizeAside is false", () => {
        render(
            <ChessboardWithSidebar
                chessboard={<div data-testid="board" />}
                aside={<div data-testid="sidebar" />}
            />,
        );

        const main = screen.getByRole("main");
        expect(main).toHaveClass("flex-col");
        expect(main).not.toHaveClass("flex-col-reverse");
    });
});
