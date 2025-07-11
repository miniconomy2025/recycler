import { Outlet, Link } from "react-router-dom"

export const Layout = () => {
    return (
        <>
            <nav>
                <Link to="/">Home</Link>
                <Link to="/revenue">Revenue</Link>
                <Link to="/stock">Stock</Link>
                <Link to="/log">Logs</Link>
            </nav>
            <main>
                <Outlet />
            </main>
        </>
    )
}