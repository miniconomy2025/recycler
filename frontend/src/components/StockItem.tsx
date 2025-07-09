export interface StockItemProps {
    displayName: string;
    quantity?: number;
    unit?: string;
}

export function StockItem(props: StockItemProps) {
    return (
        <div className="eco-card">
            <h1>{props.displayName}</h1>
            <h2>Quantity: {props.quantity?.toString() ?? "NA"} {props.unit}</h2>
        </div>
    );
}