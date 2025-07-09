import { useEffect, useState } from "react";
import { StockItem } from "../components/StockItem";
import { fetcher } from "../utils/fetcher";

export const Stock = () => {
    const [stock, setStock] = useState<{ rawMaterials: any[]; phones: any[] }>({
        rawMaterials: [],
        phones: [],
    });
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStock = async () => {
            try {
                const data = await fetcher('/stock');
                setStock(data);
            } catch (err: any) {
                setError(`Failed to fetch stock: ${err.status || 'Unknown error'}`);
            } finally {
                setLoading(false);
            }
        };

        fetchStock();
    }, []);

    return (
        <>
            <h1>Current Stock</h1>

            {loading && <p>Loading...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}

            {!loading && !error && (
                <>
                    <h2>Raw Materials</h2>
                    {stock.rawMaterials.map((item, index) => (
                        <StockItem
                            key={`raw-${index}`}
                            displayName={item.name}
                            quantity={item.quantity}
                            unit={item.unit}
                        />
                    ))}

                    <h2>Phones</h2>
                    {stock.phones.map((item, index) => (
                        <StockItem
                            key={`phone-${index}`}
                            displayName={item.name}
                            quantity={item.quantity}
                            unit={item.unit}
                        />
                    ))}
                </>
            )}
        </>
    );
};
