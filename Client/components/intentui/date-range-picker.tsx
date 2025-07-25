"use client"

import {
  DateRangePicker as DateRangePickerPrimitive,
  type DateRangePickerProps as DateRangePickerPrimitiveProps,
  type DateValue,
  type ValidationResult,
} from "react-aria-components"

import { composeTailwindRenderProps } from "@/lib/primitive"
import type { DateDuration } from "@internationalized/date"
import type { Placement } from "react-aria"
import { DateInput } from "./date-field"
import { DatePickerIcon, DatePickerOverlay } from "./date-picker"
import { Description, FieldError, FieldGroup, Label } from "./field"

interface DateRangePickerProps<T extends DateValue> extends DateRangePickerPrimitiveProps<T> {
  label?: string
  description?: string
  errorMessage?: string | ((validation: ValidationResult) => string)
  visibleDuration?: DateDuration
  pageBehavior?: "visible" | "single"
  contentPlacement?: Placement
}

const DateRangePicker = <T extends DateValue>({
  label,
  className,
  description,
  errorMessage,
  contentPlacement = "bottom",
  visibleDuration = { months: 1 },
  ...props
}: DateRangePickerProps<T>) => {
  return (
    <DateRangePickerPrimitive
      {...props}
      className={composeTailwindRenderProps(
        className,
        "group/date-range-picker flex flex-col gap-y-1",
      )}
    >
      {label && <Label>{label}</Label>}
      <FieldGroup className="w-auto min-w-40">
        <DateInput slot="start" />
        <span
          aria-hidden="true"
          className="text-neutral-content group-disabled:text-neutral-content forced-colors:text-[ButtonText] forced-colors:group-disabled:text-[GrayText]"
        >
          –
        </span>
        <DateInput className="pe-8" slot="end" />
        <DatePickerIcon />
      </FieldGroup>
      {description && <Description>{description}</Description>}
      <FieldError>{errorMessage}</FieldError>
      <DatePickerOverlay placement={contentPlacement} visibleDuration={visibleDuration} range />
    </DateRangePickerPrimitive>
  )
}
export type { DateRangePickerProps }
export { DateRangePicker }
