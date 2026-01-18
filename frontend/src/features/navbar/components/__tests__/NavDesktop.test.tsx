import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    SIDEBAR_COLLAPSED_CLS,
    SIDEBAR_EXPANDED_CLS,
} from "../../lib/sidebarWidth";

import useCollapseState from "../../hooks/useCollapseState";
import NavDesktop from "../NavDesktop";

vi.mock("../../hooks/useCollapseState");
vi.mock("../NavItems");

describe("NavDesktop", () => {
    const useCollapseStateMock = vi.mocked(useCollapseState);
    const toggleCollapse = vi.fn();
    let isCollapsed = false;

    beforeEach(() => {
        useCollapseStateMock.mockImplementation(() => ({
            isCollapsed,
            toggleCollapse,
        }));
    });

    it("should render sidebar inside the desktop nav", () => {
        render(<NavDesktop />);
        const sidebar = screen.getByTestId("sidebar");
        expect(sidebar).toBeInTheDocument();
    });

    it("should apply the expanded class when initial state is not collapsed", () => {
        isCollapsed = false;
        render(<NavDesktop />);
        const navDesktop = screen.getByTestId("navDesktop");
        expect(navDesktop).toHaveClass(SIDEBAR_EXPANDED_CLS);
    });

    it("should apply the collapsed class when initial state is collapsed", () => {
        isCollapsed = true;
        render(<NavDesktop />);
        const navDesktop = screen.getByTestId("navDesktop");
        expect(navDesktop).toHaveClass(SIDEBAR_COLLAPSED_CLS);
    });

    it("should call toggleCollapse when collapse button is clicked", async () => {
        isCollapsed = false;
        const user = userEvent.setup();

        render(<NavDesktop />);

        const button = screen.getByTestId("sidebarCollapseButton");
        await user.click(button);
        expect(toggleCollapse).toHaveBeenCalled();
    });
});
