import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import yandexLogo from "../assets/yandexLogo.svg";
import googleLogo from "../assets/googleLogo.svg";

function Login() {
  const navigate = useNavigate();

  const [email, setEmail] = useState<string>(" ");
  const [password, setPassword] = useState<string>("");
  const [error, setError] = useState<string>("");

  useEffect(() => {
    if (localStorage.accessToken) navigate("/");
  });

  async function submit() {
    setError("");
    let data = {
      email: email,
      password: password,
    };

    let response = await fetch("https://localhost:7210/api/auth/login", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    let json = await response.json();
    if (response.ok) {
      localStorage.setItem("accessToken", json.accessToken);
      localStorage.setItem("refreshToken", json.refreshToken);
      window.location.reload();
      navigate("/");
      return;
    } else {
      setError(`Error: ${json}`);
    }
  }
  return (
    <>
      <h2>
        Login or{" "}
        <a
          className="button"
          href="https://localhost:7210/api/auth/yandex/login?state=http://localhost:5173/yandex/confirm"
        >
          <img width="22px" src={yandexLogo} />
        </a>
        <a
          className="button"
          href="https://localhost:7210/api/auth/google/login?state=http://localhost:5173/google/confirm"
        >
          <img width="22px" src={googleLogo} />
        </a>
      </h2>

      <p>{error}</p>
      <form>
        <input
          type="email"
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Email"
          name="email"
          autoComplete="on"
        ></input>
        <p></p>
        <input
          type="password"
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Password"
          autoComplete="on"
        ></input>
        <p></p>
        <div className="button" onClick={submit}>
          Send
        </div>
      </form>
    </>
  );
}

export default Login;
