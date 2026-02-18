import { TimeControl } from "@/lib/apiClient";
import { render, screen } from "@testing-library/react";
import TimeControlIconFromSeconds from "../TimeControlIconFromSeconds";

vi.mock("../TimeControlIcon", () => ({
    default: ({
        timeControl,
        className,
    }: {
        timeControl: TimeControl;
        className?: string;
    }) => (
        <div
            data-testid="timeControlIcon"
            data-timecontrol={timeControl}
            className={className}
        />
    ),
}));

describe("TimeControlIconFromSeconds", () => {
    it.each([
        [179, TimeControl.BULLET],
        [180, TimeControl.BLITZ],
        [300, TimeControl.BLITZ],
        [301, TimeControl.RAPID],
        [1200, TimeControl.RAPID],
        [1201, TimeControl.CLASSICAL],
    ])(
        "should render correct time control for %i seconds",
        (baseSeconds, expectedTimeControl) => {
            render(<TimeControlIconFromSeconds baseSeconds={baseSeconds} />);

            expect(screen.getByTestId("timeControlIcon")).toHaveAttribute(
                "data-timecontrol",
                expectedTimeControl.toString(),
            );
        },
    );

    it("should forward className to TimeControlIcon", () => {
        render(
            <TimeControlIconFromSeconds
                baseSeconds={60}
                className="test-class"
            />,
        );

        expect(screen.getByTestId("timeControlIcon")).toHaveClass("test-class");
    });
});
