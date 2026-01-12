import Link from "next/link";

import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const DailyQuestCardLoggedOut = () => {
    return (
        <Card className="p-6">
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
        </Card>
    );
};
export default DailyQuestCardLoggedOut;
