import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { act } from "react";

import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { checkBotHealth } from "@/lib/apiClient";
import constants from "@/lib/constants";
import BotOffline from "../BotOffline";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

vi.mock("@/lib/apiClient/definition");

describe("BotOffline", () => {
    let routerMock: RouterMock;

    const checkBotHealthMock = vi.mocked(checkBotHealth);

    beforeEach(() => {
        routerMock = mockRouter();

        checkBotHealthMock.mockResolvedValue({
            data: false,
            error: undefined,
            response: new Response(),
        });
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    it("should automatically check bot health on mount and navigate if online", async () => {
        checkBotHealthMock.mockResolvedValue({
            error: undefined,
            data: true,
            response: new Response(),
        });

        render(<BotOffline />);
        await flushMicrotasks();

        expect(checkBotHealthMock).toHaveBeenCalledOnce();
        expect(routerMock.replace).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.BOT,
        );
    });

    it("should render offline message and button", async () => {
        render(<BotOffline />);

        expect(screen.getByText(/Anarchy Bot Is offline/i)).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /Try Again/i }),
        ).toBeInTheDocument();
    });

    it("should navigate if try again results in bot online", async () => {
        const user = userEvent.setup();
        render(<BotOffline />);

        checkBotHealthMock.mockResolvedValue({
            error: undefined,
            data: true,
            response: new Response(),
        });

        const button = screen.getByRole("button", { name: /Try Again/i });

        await user.click(button);
        await act(() => vi.advanceTimersByTime(3000));
        expect(button).not.toBeDisabled();

        expect(checkBotHealthMock).toHaveBeenCalledTimes(2);
        expect(routerMock.replace).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.BOT,
        );
    });

    it("should not navigate if bot health check fails", async () => {
        checkBotHealthMock.mockResolvedValue({
            error: {},
            data: undefined,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(<BotOffline />);
        const button = screen.getByRole("button", { name: /Try Again/i });

        await user.click(button);
        await act(() => vi.advanceTimersByTime(3000));
        expect(button).not.toBeDisabled();

        expect(routerMock.replace).not.toHaveBeenCalled();
        expect(button).not.toBeDisabled();
    });

    it("should keep the button disabled if checkBotHealth takes less than 3 seconds", async () => {
        const user = userEvent.setup();
        render(<BotOffline />);
        const button = screen.getByRole("button", { name: /Try Again/i });

        await user.click(button);

        await act(() => vi.advanceTimersByTime(2000));
        expect(button).toBeDisabled();

        await act(() => vi.advanceTimersByTime(1000));
        expect(button).not.toBeDisabled();
    });

    it("should keep the button disabled if checkBotHealth takes longer than 3 seconds", async () => {
        checkBotHealthMock.mockImplementation(
            () =>
                new Promise(
                    (resolve) =>
                        setTimeout(
                            () =>
                                resolve({
                                    error: undefined,
                                    data: true,
                                    response: new Response(),
                                }),
                            5000,
                        ),
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                ) as any,
        );

        const user = userEvent.setup();
        render(<BotOffline />);
        const button = screen.getByRole("button", { name: /Try Again/i });

        await user.click(button);

        await act(() => vi.advanceTimersByTime(3000));
        expect(button).toBeDisabled();

        await act(() => vi.advanceTimersByTime(2000));
        expect(button).not.toBeDisabled();
    });
});
