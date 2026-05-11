import { render, screen } from "@testing-library/react";

import ChangeLogMonthDivider from "../ChangeLogMonthDivider";

describe("ChangeLogMonthDivider", () => {
    it("should render the provided date", () => {
        render(<ChangeLogMonthDivider date="May 2026" />);

        expect(screen.getByText("May 2026")).toBeInTheDocument();
    });

    it("should render a horizontal divider", () => {
        const { container } = render(<ChangeLogMonthDivider date="May 2026" />);

        expect(container.querySelector("hr")).toBeInTheDocument();
    });
});
