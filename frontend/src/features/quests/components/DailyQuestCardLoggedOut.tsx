import Button from "@/components/ui/Button";
import constants from "@/lib/constants";
import Link from "next/link";

const DailyQuestCardLoggedOut = () => {
    return (
        <>
            <div className="flex flex-col gap-4">
                <p
                    className="text-text/70 text-lg"
                    data-testid="dailyQuestLoggedOutMessage"
                >
                    Sign In to start completing daily quests, earn streaks, and
                    track your progress!
                </p>

                <Button>
                    <Link
                        href={constants.PATHS.SIGNIN}
                        className="flex h-full w-full items-center
                            justify-center"
                        data-testid="dailyQuestLoggedOutSignInLink"
                    >
                        Sign In
                    </Link>
                </Button>
            </div>
        </>
    );
};
export default DailyQuestCardLoggedOut;
