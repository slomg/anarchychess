import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const DailyQuestTitle = () => {
    return (
        <Card className="justify-center" data-testid="dailyQuestHeader">
            <h1 className="text-4xl">Daily Quest</h1>
            <h2 className="text-text/70 text-2xl">
                {constants.QUEST_WEEKDAY_NAMES[new Date().getDay()]}
            </h2>
        </Card>
    );
};
export default DailyQuestTitle;
