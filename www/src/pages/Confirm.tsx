import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

function Confirm() {
  const navigate = useNavigate();

  const [email, setEmail] = useState<string>(" ");
  const [code, setCode] = useState<string>("");
  const [error, setError] = useState<string>("");

  async function submit() {
    setError("");
    let data = {
      email: email,
      requestCode: code,
    };

    let response = await fetch(
      "https://localhost:7210/api/auth/register/confirm",
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      }
    );

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

  useEffect(() => {
    if (localStorage.accessToken) navigate("/");
  });
  return (
    <>
      <h2>Confirm code from email</h2>
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
          type="text"
          onChange={(e) => setCode(e.target.value)}
          placeholder="Code"
          autoComplete="off"
        ></input>
        <p></p>
        <div className="button" onClick={submit}>
          Send
        </div>
      </form>
    </>
  );
}

export default Confirm;
