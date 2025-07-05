import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import App from './App';
import { Revenue } from './pages/Revenue';


export const AppWrapper = () => (
    <Router>
        <Routes>
            <Route path="/home" element={<App />} />
            <Route path='/revenue' element={<Revenue/>} />
        </Routes>
    </Router>
);