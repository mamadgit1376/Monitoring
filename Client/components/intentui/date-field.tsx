"use client"

import { composeTailwindRenderProps } from "@/lib/primitive"
import {
  DateField as DateFieldPrimitive,
  type DateFieldProps as DateFieldPrimitiveProps,
  DateInput as DateInputPrimitive,
  type DateInputProps,
  DateSegment,
  type DateValue,
  type ValidationResult,
} from "react-aria-components"
import { tv } from "tailwind-variants"
import { Description, FieldError, FieldGroup, Label } from "./field"

interface DateFieldProps<T extends DateValue> extends DateFieldPrimitiveProps<T> {
  label?: string
  description?: string
  errorMessage?: string | ((validation: ValidationResult) => string)
  prefix?: React.ReactNode
  suffix?: React.ReactNode
}

const DateField = <T extends DateValue>({
  prefix,
  suffix,
  label,
  description,
  errorMessage,
  ...props
}: DateFieldProps<T>) => {
  return (
    <DateFieldPrimitive
      {...props}
      className={composeTailwindRenderProps(props.className, "group flex flex-col gap-y-1.5")}
    >
      {label && <Label>{label}</Label>}
      <FieldGroup>
        {prefix && typeof prefix === "string" ? (
          <span className="ms-2 text-neutral-content">{prefix}</span>
        ) : (
          prefix
        )}
        <DateInput />
        {suffix ? (
          typeof suffix === "string" ? (
            <span className="me-2 text-neutral-content">{suffix}</span>
          ) : (
            suffix
          )
        ) : null}
      </FieldGroup>
      {description && <Description>{description}</Description>}
      <FieldError>{errorMessage}</FieldError>
    </DateFieldPrimitive>
  )
}

const segmentStyles = tv({
  base: "inline shrink-0 rounded p-0.5 type-literal:px-0 text-neutral-content tabular-nums tracking-wider caret-transparent outline-0 forced-color-adjust-none sm:text-sm forced-colors:text-[ButtonText]",
  variants: {
    isPlaceholder: {
      true: "text-neutral-content",
    },
    isDisabled: {
      true: "text-neutral-content/50 forced-colors:text-[GrayText]",
    },
    isFocused: {
      true: [
        "bg-primary text-primary-content forced-colors:bg-[Highlight] forced-colors:text-[HighlightText]",
        "data-invalid:bg-danger data-invalid:text-danger-fg",
      ],
    },
  },
})

const DateInput = ({ className, ...props }: Omit<DateInputProps, "children">) => {
  return (
    <DateInputPrimitive
      className={composeTailwindRenderProps(
        className,
        "bg-transparent p-2 text-base text-neutral-content placeholder-muted-fg",
      )}
      {...props}
    >
      {(segment) => <DateSegment segment={segment} className={segmentStyles} />}
    </DateInputPrimitive>
  )
}

export type { DateFieldProps }
export { DateField, DateInput, segmentStyles }
