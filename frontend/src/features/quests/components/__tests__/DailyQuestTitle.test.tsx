import { render, within, screen } from "@testing-library/react";

import DailyQuestTitle from "../DailyQuestTitle";
import constants from "@/lib/constants";

describe("DailyQuestTitle", () => {
    it("should render the title with the current weekday", () => {
        render(<DailyQuestTitle />);

        const header = screen.getByTestId("dailyQuestHeader");
        expect(within(header).getByText("Daily Quest")).toBeInTheDocument();
        expect(
            within(header).getByText(
                constants.QUEST_WEEKDAY_NAMES[new Date().getDay()],
            ),
        ).toBeInTheDocument();
    });
});
