import { useState } from "react";
import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import Login from "./pages/Login";
import Home from "./pages/Home";
import Register from "./pages/Register";
import Confirm from "./pages/Confirm";
import YandexConfirm from "./pages/YandexConfirm";
import GoogleConfirm from "./pages/GoogleConfirm";

import "./App.css";

function App() {
  const [authorize, setAuthorize] = useState<boolean>(localStorage.accessToken);

  async function logount() {
    await fetch("https://localhost:7210/api/auth/logout", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        Authorization: "Bearer " + localStorage.getItem("accessToken"),
      },
    });
    localStorage.clear();
    setAuthorize(false);
  }
  return (
    <BrowserRouter>
      <main>
        <div className="header">
          <Link to="/">Home</Link>
          {!authorize ? (
            <>
              <Link to="/login">Login</Link>
              <Link to="/Register">Registration</Link>
            </>
          ) : (
            <>
              <div>
                Your - authorized :{" "}
                <div className="button" onClick={logount}>
                  Logout
                </div>
              </div>
            </>
          )}
        </div>
        <Routes>
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="register/confirm" element={<Confirm />} />
          <Route path="yandex/confirm" element={<YandexConfirm />} />
          <Route path="google/confirm" element={<GoogleConfirm />} />
          <Route path="*" element={<Home />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}

export default App;
