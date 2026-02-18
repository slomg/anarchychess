import { TimeControl } from "@/lib/apiClient";
import TimeControlIcon from "./TimeControlIcon";

const TimeControlIconFromSeconds = ({
    baseSeconds,
    className,
}: {
    baseSeconds: number;
    className?: string;
}) => {
    if (baseSeconds < 180) {
        return (
            <TimeControlIcon
                timeControl={TimeControl.BULLET}
                className={className}
            />
        );
    } else if (baseSeconds <= 300) {
        return (
            <TimeControlIcon
                timeControl={TimeControl.BLITZ}
                className={className}
            />
        );
    } else if (baseSeconds <= 1200) {
        return (
            <TimeControlIcon
                timeControl={TimeControl.RAPID}
                className={className}
            />
        );
    } else {
        return (
            <TimeControlIcon
                timeControl={TimeControl.CLASSICAL}
                className={className}
            />
        );
    }
};
export default TimeControlIconFromSeconds;
