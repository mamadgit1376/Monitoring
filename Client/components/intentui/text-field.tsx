"use client"

import { useState } from "react"

import { composeTailwindRenderProps } from "@/lib/primitive"
import { IconEye, IconEyeClosed } from "@intentui/icons"
import { Button as ButtonPrimitive, TextField as TextFieldPrimitive } from "react-aria-components"
import type { InputProps, TextFieldProps as TextFieldPrimitiveProps } from "react-aria-components"
import type { FieldProps } from "./field"
import { Description, FieldError, FieldGroup, Input, Label } from "./field"
import { Loader } from "./loader"

type InputType = Exclude<InputProps["type"], "password">

interface BaseTextFieldProps extends TextFieldPrimitiveProps, FieldProps {
  prefix?: React.ReactNode
  suffix?: React.ReactNode
  isPending?: boolean
  className?: string
}

interface RevealableTextFieldProps extends BaseTextFieldProps {
  isRevealable: true
  type: "password"
}

interface NonRevealableTextFieldProps extends BaseTextFieldProps {
  isRevealable?: never
  type?: InputType
}

type TextFieldProps = RevealableTextFieldProps | NonRevealableTextFieldProps

const TextField = ({
  placeholder,
  label,
  description,
  errorMessage,
  prefix,
  suffix,
  isPending,
  className,
  isRevealable,
  type,
  ...props
}: TextFieldProps) => {
  const [isPasswordVisible, setIsPasswordVisible] = useState(false)
  const inputType = isRevealable ? (isPasswordVisible ? "text" : "password") : type
  const handleTogglePasswordVisibility = () => {
    setIsPasswordVisible((prev) => !prev)
  }
  return (
    <TextFieldPrimitive
      type={inputType}
      {...props}
      className={composeTailwindRenderProps(className, "group flex flex-col gap-y-1")}
    >
      {!props.children ? (
        <>
          {label && <Label>{label}</Label>}
          <FieldGroup
            isDisabled={props.isDisabled}
            isInvalid={!!errorMessage}
            data-loading={isPending ? "true" : undefined}
          >
            {prefix && typeof prefix === "string" ? (
              <span className="ms-2 text-neutral-content">{prefix}</span>
            ) : (
              prefix
            )}
            <Input placeholder={placeholder} />
            {isRevealable ? (
              <ButtonPrimitive
                type="button"
                aria-label="Toggle password visibility"
                onPress={handleTogglePasswordVisibility}
                className="relative me-1 grid shrink-0 place-content-center rounded-sm border-transparent outline-hidden *:data-[slot=icon]:text-neutral-content focus-visible:*:data-[slot=icon]:text-primary"
              >
                {isPasswordVisible ? <IconEyeClosed /> : <IconEye />}
              </ButtonPrimitive>
            ) : isPending ? (
              <Loader variant="spin" />
            ) : suffix ? (
              typeof suffix === "string" ? (
                <span className="me-2 text-neutral-content">{suffix}</span>
              ) : (
                suffix
              )
            ) : null}
          </FieldGroup>
          {description && <Description>{description}</Description>}
          <FieldError>{errorMessage}</FieldError>
        </>
      ) : (
        props.children
      )}
    </TextFieldPrimitive>
  )
}

export type { TextFieldProps }
export { TextField }
