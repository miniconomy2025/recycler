import React from 'react';
import logo from './logo.svg';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Layout } from './pages/Layout';
import { Home } from './pages/Home';
import { Revenue } from './pages/Revenue';
import { Stock } from './pages/Stock';
import Log from "./pages/Log";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path='/' element={<Layout />}>
          <Route index element={<Home />} />
          <Route path='/revenue' element={<Revenue />} />
          <Route path='/stock' element={<Stock />} />
          <Route path='/log' element={<Log />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}