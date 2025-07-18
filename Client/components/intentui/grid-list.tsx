import type React from "react"

import { IconHamburger } from "@intentui/icons"
import type { GridListItemProps, GridListProps } from "react-aria-components"
import {
  Button,
  GridListItem as GridListItemPrimitive,
  GridList as GridListPrimitive,
  composeRenderProps,
} from "react-aria-components"
import { tv } from "tailwind-variants"

import { composeTailwindRenderProps } from "@/lib/primitive"
import { twMerge } from "tailwind-merge"
import { Checkbox } from "./checkbox"

const GridList = <T extends object>({ children, className, ...props }: GridListProps<T>) => (
  <GridListPrimitive
    className={composeTailwindRenderProps(
      className,
      "relative max-h-96 overflow-auto rounded-lg border [scrollbar-width:thin] *:data-drop-target:border *:data-drop-target:border-accent [&::-webkit-scrollbar]:size-0.5",
    )}
    {...props}
  >
    {children}
  </GridListPrimitive>
)

const itemStyles = tv({
  base: "group -mb-px -outline-offset-2 relative flex cursor-default select-none gap-3 border-y px-3 py-2 text-neutral-content outline-hidden transition [--selected-item-hovered:theme(--color-muted/70%)] [--selected-item:theme(--color-muted/80%)] first:rounded-t-md first:border-t-0 last:mb-0 last:rounded-b-md last:border-b-0 sm:text-sm",
  variants: {
    isHovered: { true: "bg-subtle" },
    isSelected: {
      true: "z-20 border-border/50 bg-(--selected-item) hover:bg-(--selected-item-hovered)",
    },
    isFocused: {
      true: "outline-hidden",
    },
    isFocusVisible: {
      true: "bg-(--selected-item) selected:bg-(--selected-item) outline-hidden ring-1 ring-ring hover:bg-(--selected-item-hovered)",
    },
    isDisabled: {
      true: "text-neutral-content/70 forced-colors:text-[GrayText]",
    },
  },
})

const GridListItem = ({ className, children, ...props }: GridListItemProps) => {
  const textValue = props.textValue || (typeof children === "string" ? children : undefined)
  return (
    <GridListItemPrimitive
      textValue={textValue}
      {...props}
      className={composeRenderProps(className, (className, renderProps) =>
        itemStyles({ ...renderProps, className }),
      )}
    >
      {(values) => (
        <>
          {values.allowsDragging && (
            <Button
              slot="drag"
              className="cursor-grab data-dragging:cursor-grabbing *:data-[slot=icon]:text-neutral-content"
            >
              <IconHamburger />
            </Button>
          )}

          <span
            aria-hidden
            className="absolute inset-y-0 start-0 hidden h-full w-0.5 bg-primary group-selected:block"
          />
          {values.selectionMode === "multiple" && values.selectionBehavior === "toggle" && (
            <Checkbox className="-me-2" slot="selection" />
          )}
          {typeof children === "function" ? children(values) : children}
        </>
      )}
    </GridListItemPrimitive>
  )
}

const GridEmptyState = ({ ref, className, ...props }: React.ComponentProps<"div">) => (
  <div ref={ref} className={twMerge("p-6", className)} {...props} />
)

GridList.Item = GridListItem
GridList.EmptyState = GridEmptyState

export type { GridListProps, GridListItemProps }
export { GridList }
