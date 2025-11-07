import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import { BrowserRouter, Routes, Route } from 'react-router-dom';

import './index.css'

import Topbar from './components/topbar'

import Home from './pages/home/home'
import { Chat } from './pages/chat/chat';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Topbar />

    <BrowserRouter>
      <Routes>
        <Route path='/' element={<Home />} />
        <Route path='/chat/:id' element={<Chat />} />
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
