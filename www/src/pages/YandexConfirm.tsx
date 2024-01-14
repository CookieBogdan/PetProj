import { useEffect } from "react";

import { useNavigate } from "react-router-dom";

function YandexConfirm() {
  const navigate = useNavigate();
  useEffect(() => {
    return () => {
      send();
    };
  }, []);

  async function send() {
    let jwt = document.cookie.split("=")[1];
    deleteAllCookies();
    let response = await fetch(
      "https://localhost:7210/api/auth/yandex/confirm",
      {
        method: "GET",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
          jwt: jwt,
        },
      }
    );
    let json = await response.json();

    if (response.ok) {
      localStorage.setItem("accessToken", json.accessToken);
      localStorage.setItem("refreshToken", json.refreshToken);

      navigate("/");
      window.location.reload();
      return;
    } else {
      alert(`Error: ${json}`);
    }
  }
  function deleteAllCookies() {
    const cookies = document.cookie.split(";");

    for (let i = 0; i < cookies.length; i++) {
      const cookie = cookies[i];
      const eqPos = cookie.indexOf("=");
      const name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
      document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
  }
  return (
    <>
      <h1></h1>wait yandex...
    </>
  );
}
export default YandexConfirm;
