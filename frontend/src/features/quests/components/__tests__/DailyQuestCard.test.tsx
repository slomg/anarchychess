import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";
import {
    collectQuestReward,
    GuestUser,
    PrivateUser,
    QuestDifficulty,
    replaceDailyQuest,
} from "@/lib/apiClient";

import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { createFakeQuest } from "@/lib/testUtils/fakers/questFaker";
import DailyQuestCard from "../DailyQuestCard";

vi.mock("@/lib/apiClient/definition");

describe("DailyQuestCard", () => {
    const replaceDailyQuestMock = vi.mocked(replaceDailyQuest);
    const collectQuestRewardMock = vi.mocked(collectQuestReward);

    let authedUserMock: PrivateUser;
    let guestUserMock: GuestUser;

    let routerMock: RouterMock;

    beforeEach(() => {
        routerMock = mockRouter();
        authedUserMock = createFakePrivateUser();
        guestUserMock = createFakeGuestUser();
    });

    it("should render the fire emoji when streak > 0", () => {
        const quest = createFakeQuest({ streak: 7 });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "🔥7 Days Streak",
        );
    });

    it("should render the description with difficulty text", () => {
        const quest = createFakeQuest({
            difficulty: QuestDifficulty.HARD,
        });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveTextContent(
            "Hard:",
        );
        expect(screen.getByTestId("dailyQuestDescription")).toHaveTextContent(
            quest.description,
        );
    });

    it("should render progress bar with correct width", () => {
        const quest = createFakeQuest({ progress: 3, target: 6 });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("progressBarFill")).toHaveStyle({
            width: "50%",
        });
    });

    it("should render progress text correctly", () => {
        const quest = createFakeQuest({ progress: 2, target: 5 });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestProgressText")).toHaveTextContent(
            "2/5",
        );
    });

    it("should render replace button", () => {
        const quest = createFakeQuest();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestReplaceButton")).toHaveTextContent(
            "Replace",
        );
    });

    it.each([
        [QuestDifficulty.EASY, "text-green-400"],
        [QuestDifficulty.MEDIUM, "text-yellow-400"],
        [QuestDifficulty.HARD, "text-red-400"],
    ])("should apply correct difficulty color class", (difficulty, style) => {
        const quest = createFakeQuest({ difficulty });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveClass(style);
    });

    it("should call replaceDailyQuest and update quest on success", async () => {
        const initialQuest = createFakeQuest({ canReplace: true });
        const newQuest = createFakeQuest({
            description: "New quest",
            canReplace: false,
        });

        replaceDailyQuestMock.mockResolvedValue({
            data: newQuest,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={initialQuest} />
            </SessionProvider>,
        );
        const replaceButton = screen.getByTestId("dailyQuestReplaceButton");

        await user.click(replaceButton);

        expect(replaceButton).not.toBeInTheDocument();

        expect(screen.getByTestId("dailyQuestDescription")).toHaveTextContent(
            newQuest.description,
        );
        expect(routerMock.refresh).toHaveBeenCalled();

        expect(replaceButton).not.toBeInTheDocument();
    });

    it("should display error message if replaceDailyQuest fails", async () => {
        const initialQuest = createFakeQuest({ canReplace: true });

        replaceDailyQuestMock.mockResolvedValue({
            data: undefined,
            error: { errors: [], extensions: {} },
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={initialQuest} />
            </SessionProvider>,
        );
        const replaceButton = screen.getByTestId("dailyQuestReplaceButton");

        await user.click(replaceButton);

        expect(screen.getByTestId("dailyQueryError")).toHaveTextContent(
            "Failed to replace quest",
        );
        expect(replaceButton).not.toBeDisabled();
    });

    it("should not render replace button if quest cannot be replaced", () => {
        const quest = createFakeQuest({ canReplace: false });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(
            screen.queryByTestId("dailyQuestReplaceButton"),
        ).not.toBeInTheDocument();
    });

    it("should render collect reward button when quest is completed and reward not collected", () => {
        const quest = createFakeQuest({
            progress: 5,
            target: 5,
            rewardCollected: false,
        });
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("dailyQuestCollectButton"),
        ).toBeInTheDocument();
        expect(screen.getByTestId("dailyQuestCollectButton")).toHaveTextContent(
            "Collect Reward",
        );
    });

    it("should call collectQuestReward and update quest on success", async () => {
        const quest = createFakeQuest({
            progress: 5,
            target: 5,
            rewardCollected: false,
        });

        collectQuestRewardMock.mockResolvedValue({
            data: quest.difficulty,
            error: undefined,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );
        const collectButton = screen.getByTestId("dailyQuestCollectButton");

        await user.click(collectButton);

        expect(collectButton).not.toBeInTheDocument();
        expect(routerMock.refresh).toHaveBeenCalled();
        expect(
            screen.getByTestId("dailyQuestCollectedRewardText"),
        ).toHaveTextContent(`+${quest.difficulty} points`);
    });

    it("should not show + points for guests", async () => {
        const quest = createFakeQuest({
            progress: 5,
            target: 5,
            rewardCollected: false,
        });

        collectQuestRewardMock.mockResolvedValue({
            data: quest.difficulty,
            error: undefined,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={guestUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );
        const collectButton = screen.getByTestId("dailyQuestCollectButton");

        await user.click(collectButton);

        expect(collectButton).not.toBeInTheDocument();
        expect(routerMock.refresh).toHaveBeenCalled();
        expect(
            screen.queryByTestId("dailyQuestCollectedRewardText"),
        ).not.toBeInTheDocument();
    });

    it("should display error message if collectQuestReward fails", async () => {
        const quest = createFakeQuest({
            progress: 5,
            target: 5,
            rewardCollected: false,
        });

        collectQuestRewardMock.mockResolvedValue({
            data: undefined,
            error: { errors: [], extensions: {} },
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );
        const collectButton = screen.getByTestId("dailyQuestCollectButton");

        await user.click(collectButton);

        expect(screen.getByTestId("dailyQueryError")).toHaveTextContent(
            "Failed to collect reward",
        );
        expect(
            screen.queryByTestId("dailyQuestCollectedRewardText"),
        ).not.toBeInTheDocument();
        expect(collectButton).not.toBeDisabled();
    });

    it("should increment streak when quest is completed and reward is collected", async () => {
        const quest = createFakeQuest({
            progress: 5,
            target: 5,
            rewardCollected: false,
            streak: 3,
        });

        collectQuestRewardMock.mockResolvedValue({
            data: quest.difficulty,
            error: undefined,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={authedUserMock}>
                <DailyQuestCard initialQuest={quest} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "🔥3 Days Streak",
        );

        const collectButton = screen.getByTestId("dailyQuestCollectButton");
        await user.click(collectButton);

        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "🔥4 Days Streak",
        );
    });
});
