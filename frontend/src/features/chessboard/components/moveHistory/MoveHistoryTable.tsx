import MoveHistoryToolbar from "./MoveHistoryToolbar";
import NavigationButtons from "./NavigationButtons";
import MoveHistoryRows from "./MoveHistoryRows";
import Card from "@/components/ui/Card";
import FlipButton from "./FlipButton";

const MoveHistoryTable = () => {
    return (
        <Card
            className="max-h-96 w-full gap-0 overflow-hidden p-0 lg:max-h-full"
        >
            <MoveHistoryToolbar
                className="order-1 lg:order-2"
                leftActions={<NavigationButtons />}
                rightActions={<FlipButton />}
            />
            <MoveHistoryRows className="order-2 lg:order-1" />
        </Card>
    );
};
export default MoveHistoryTable;
