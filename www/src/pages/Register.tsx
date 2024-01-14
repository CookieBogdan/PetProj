import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import yandexLogo from "../assets/yandexLogo.svg";
import googleLogo from "../assets/googleLogo.svg";

function Register() {
  const navigate = useNavigate();

  const [email, setEmail] = useState<string>(" ");
  const [password, setPassword] = useState<string>("");
  const [error, setError] = useState<string>("");

  async function submit() {
    setError("");
    let data = {
      email: email,
      password: password,
    };

    let response = await fetch("https://localhost:7210/api/auth/register", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    if (response.ok) {
      navigate("/register/confirm");
    } else {
      let json = await response.json();
      setError(`Error: ${json}`);
    }
  }

  useEffect(() => {
    if (localStorage.accessToken) navigate("/");
  });
  return (
    <>
      <h2>
        Registration or{" "}
        <a
          className="button"
          href="https://localhost:7210/api/auth/yandex/login?state=http://localhost:5173/yandex/confirm"
        >
          <img width="22px" src={yandexLogo} />
        </a>
        <div className="button">
          <img width="22px" src={googleLogo} />
        </div>
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

export default Register;
