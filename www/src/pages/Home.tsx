import { useNavigate } from "react-router-dom";

const baseUrl = "https://localhost:7210";

function Home() {
  const navigate = useNavigate();

  async function hello(i: number) {
    fetch(baseUrl + "/hello", {
      method: "GET",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        Authorization: "Bearer " + localStorage.getItem("accessToken"),
      },
    })
      .then(function (response) {
        if (!response.ok) {
          if (response.status === 401) {
            if (i !== 1) refresh(hello);
          } else {
            alert(response.json());
          }
          return;
        }
        return response.json();
      })
      .then(function (data) {
        if (data != undefined) alert(data);
      });
  }

  async function refresh(callback: Function) {
    if (!localStorage.accessToken) {
      navigate("/login");
      return;
    }
    console.log("refresh");
    fetch(baseUrl + "/api/auth/refresh", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        Authorization: "Bearer " + localStorage.getItem("accessToken"),
      },
      body: JSON.stringify(localStorage.getItem("refreshToken")),
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (json) {
        localStorage.setItem("accessToken", json.accessToken);
        localStorage.setItem("refreshToken", json.refreshToken);
        callback(1);
      })
      .catch(function () {
        navigate("/login");
      });
  }

  return (
    <>
      <h2>Api Tests:</h2>
      <div className="button" onClick={() => hello(0)}>
        hello
      </div>
    </>
  );
}

export default Home;
