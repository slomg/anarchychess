import { twMerge } from "tailwind-merge";

const AnalysisPositionSetup = ({ className }: { className?: string }) => {
    return (
        <div
            className={twMerge("flex-1", className)}
            data-testid="analysisPositionSetup"
        ></div>
    );
};
export default AnalysisPositionSetup;
