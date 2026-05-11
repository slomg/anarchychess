import { render, screen } from "@testing-library/react";

import ChangeLogCard, {
    CHANGELOG_TYPE_COLORS,
    ChangeLogType,
} from "../ChangeLogCard";

describe("ChangeLogCard", () => {
    it("should render the changelog type", () => {
        render(<ChangeLogCard type={ChangeLogType.FEATURE} date="May 2026" />);

        expect(screen.getByText("FEATURE")).toBeInTheDocument();
    });

    it("should render the provided date", () => {
        render(<ChangeLogCard type={ChangeLogType.FEATURE} date="May 2026" />);

        expect(screen.getByText("May 2026")).toBeInTheDocument();
    });

    it("should render the children content", () => {
        render(
            <ChangeLogCard type={ChangeLogType.FEATURE} date="May 2026">
                test change
            </ChangeLogCard>,
        );

        expect(screen.getByText("test change")).toBeInTheDocument();
    });

    it.each([
        ChangeLogType.RULE,
        ChangeLogType.FIX,
        ChangeLogType.TWEAK,
        ChangeLogType.FEATURE,
    ])("should apply the correct colors for change log type", (type) => {
        render(<ChangeLogCard type={type} date="May 2026" />);

        const badge = screen.getByText(ChangeLogType[type]);

        expect(badge).toHaveStyle({
            backgroundColor: CHANGELOG_TYPE_COLORS[type].background,
            borderColor: CHANGELOG_TYPE_COLORS[type].border,
        });
    });
});
