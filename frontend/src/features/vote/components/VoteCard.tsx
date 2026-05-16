import clsx from "clsx";

import { VoteOption } from "@/lib/apiClient";
import Card from "@/components/ui/Card";

const VoteCard = ({
    option,
    optionLetter,
    isSelected,
    onClick,
}: {
    option: VoteOption;
    optionLetter: string;
    isSelected: boolean;
    onClick: () => void;
}) => {
    return (
        <Card
            className={clsx(
                `w-full cursor-pointer gap-4 transition-transform
                hover:scale-102`,
                isSelected && "outline-secondary outline-3",
            )}
            onClick={onClick}
        >
            <div className="flex items-center gap-2">
                <span
                    className="bg-primary flex h-8 w-8 items-center
                        justify-center rounded-full"
                >
                    {optionLetter}
                </span>
                <span className="text-sm">OPTION {optionLetter}</span>
            </div>

            <div>
                <h1 className="text-2xl">{option.name}</h1>
                <p>{option.description}</p>
            </div>
        </Card>
    );
};
export default VoteCard;
