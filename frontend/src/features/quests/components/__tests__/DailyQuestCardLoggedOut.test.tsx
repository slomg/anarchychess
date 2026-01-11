import { render, screen } from "@testing-library/react";

import DailyQuestCardLoggedOut from "../DailyQuestCardLoggedOut";
import constants from "@/lib/constants";

describe("DailyQuestCardLoggedOut", () => {
    it("should render the sign in message", () => {
        render(<DailyQuestCardLoggedOut />);

        expect(
            screen.getByTestId("dailyQuestLoggedOutMessage"),
        ).toHaveTextContent(/sign in to start completing daily quests/i);
    });

    it("should link the button to the sign in page", () => {
        render(<DailyQuestCardLoggedOut />);

        const link = screen.getByTestId("dailyQuestLoggedOutSignInLink");
        expect(link).toHaveAttribute("href", constants.PATHS.SIGNIN);
        expect(link).toHaveTextContent("Sign In");
    });
});
