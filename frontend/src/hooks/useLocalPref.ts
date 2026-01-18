import {
    SetStateAction,
    useCallback,
    useEffect,
    useEffectEvent,
    useState,
} from "react";

export default function useLocalPref<T>(
    localStorageName: string,
    defaultValue: T,
): [T, (newValue: SetStateAction<T>) => void] {
    const [value, setValue] = useState<T>(defaultValue);

    const setValueFromStorage = useEffectEvent(() => {
        const storageValue = localStorage.getItem(localStorageName);
        if (!storageValue) return;
        setValue(JSON.parse(storageValue));
    });
    useEffect(() => setValueFromStorage(), []);

    const setNewValue = useCallback(
        (newValue: React.SetStateAction<T>) => {
            setValue((prev) => {
                const resolvedValue =
                    typeof newValue === "function"
                        ? (newValue as (prev: T) => T)(prev)
                        : newValue;
                localStorage.setItem(
                    localStorageName,
                    JSON.stringify(resolvedValue),
                );

                return resolvedValue;
            });
        },
        [localStorageName],
    );

    return [value, setNewValue];
}
