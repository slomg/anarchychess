import { useEffect, useEffectEvent, useState } from "react";

export default function useLocalPref<T>(
    localStorageName: string,
    defaultValue: T,
): [T, (newValue: T) => void] {
    const [value, setValue] = useState<T>(defaultValue);

    const setValueFromStorage = useEffectEvent(() => {
        const storageValue = localStorage.getItem(localStorageName);
        if (!storageValue) return;
        setValue(JSON.parse(storageValue));
    });
    useEffect(() => setValueFromStorage(), []);

    function setNewValue(newValue: T) {
        setValue(newValue);
        localStorage.setItem(localStorageName, JSON.stringify(newValue));
    }

    return [value, setNewValue];
}
