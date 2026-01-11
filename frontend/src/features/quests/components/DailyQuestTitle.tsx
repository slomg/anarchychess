import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const DailyQuestTitle = () => {
    return (
        <Card
            className="items-end justify-center"
            data-testid="dailyQuestHeader"
        >
            <h1 className="text-4xl text-wrap">Daily Quest</h1>
            <h2 className="text-text/70 text-2xl">
                {constants.QUEST_WEEKDAY_NAMES[new Date().getDay()]}
            </h2>
        </Card>
    );
};
export default DailyQuestTitle;
