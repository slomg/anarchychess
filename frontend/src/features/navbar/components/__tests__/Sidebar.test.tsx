import { render, screen } from "@testing-library/react";
import Sidebar from "../Sidebar";
import userEvent from "@testing-library/user-event";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import constants from "@/lib/constants";

vi.mock("../NavItems");
vi.mock("js-cookie");

describe("Sidebar", () => {
    it("should render the sidebar with correct data attributes", () => {
        render(<Sidebar isCollapsed={false} />);
        const sidebar = screen.getByTestId("sidebar");

        expect(sidebar).toBeInTheDocument();
        expect(sidebar).toHaveAttribute("data-is-collapsed", "false");
        expect(sidebar).toHaveAttribute("aria-label", "sidebar");
    });

    it("should reflect collapsed state in data attribute", () => {
        render(<Sidebar isCollapsed={true} />);
        const sidebar = screen.getByTestId("sidebar");
        expect(sidebar.getAttribute("data-is-collapsed")).toBe("true");
    });

    it("should render the correct logo depending on collapsed state", () => {
        const { rerender } = render(<Sidebar isCollapsed={false} />);
        expect(screen.getByAltText("Logo with text")).toBeInTheDocument();

        rerender(<Sidebar isCollapsed={true} />);
        expect(screen.getByAltText("Logo")).toBeInTheDocument();
    });

    it.each([true, false])(
        "should render UpperNavItems with correct prop",
        (isLoggedIn) => {
            mockJsCookie({
                [constants.COOKIES.IS_LOGGED_IN]: isLoggedIn.toString(),
            });

            render(<Sidebar isCollapsed={false} />);

            const upperNav = screen.getByTestId("upperNavItems");
            expect(upperNav).toBeInTheDocument();
            expect(upperNav).toHaveAttribute(
                "data-is-logged-in",
                isLoggedIn.toString(),
            );
        },
    );

    it.each([true, false])(
        "should render LowerNavItems with correct prop",
        (isLoggedIn) => {
            mockJsCookie({
                [constants.COOKIES.IS_LOGGED_IN]: isLoggedIn.toString(),
            });

            render(<Sidebar isCollapsed={false} />);

            const lowerNav = screen.getByTestId("lowerNavItems");
            expect(lowerNav).toBeInTheDocument();
            expect(lowerNav).toHaveAttribute(
                "data-is-logged-in",
                isLoggedIn.toString(),
            );
        },
    );

    it("should render the collapse button and call toggleCollapse on click", async () => {
        const toggleCollapse = vi.fn();
        const user = userEvent.setup();
        render(<Sidebar isCollapsed={false} toggleCollapse={toggleCollapse} />);
        const button = screen.getByTestId("sidebarCollapseButton");

        expect(button).toBeInTheDocument();
        await user.click(button);
        expect(toggleCollapse).toHaveBeenCalled();
    });
});
