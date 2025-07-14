"use client"
import { signOut } from "next-auth/react";

export default function logout(){
    async function handleLogout() 
    {
        signOut({callbackUrl: "https://mehrab.ir"})
    }
    return(
        <span onClick={handleLogout}
         className=" text-sm lg:text-xl py-2 btn btn-ghost  hover:scale-105 transition-transform duration-200"> 
        خروج  
        </span>
    );
}