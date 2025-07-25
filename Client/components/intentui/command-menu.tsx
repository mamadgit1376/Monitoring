"use client"

import { composeTailwindRenderProps } from "@/lib/primitive"
import { IconSearch } from "@intentui/icons"
import { createContext, use, useEffect } from "react"
import type {
  AutocompleteProps,
  CollectionRenderer,
  MenuProps,
  MenuTriggerProps,
  SearchFieldProps,
} from "react-aria-components"
import {
  Autocomplete,
  Button,
  Collection,
  CollectionRendererContext,
  DefaultCollectionRenderer,
  Dialog,
  Header,
  Input,
  Menu as MenuPrimitive,
  MenuSection,
  Modal,
  ModalContext,
  ModalOverlay,
  OverlayTriggerStateContext,
  SearchField,
  useFilter,
} from "react-aria-components"
import { twMerge } from "tailwind-merge"
import { DropdownKeyboard } from "./dropdown"
import { Loader } from "./loader"
import { Menu, type MenuSectionProps } from "./menu"

interface CommandMenuProviderProps {
  isPending?: boolean
  escapeButton?: boolean
}

const CommandMenuContext = createContext<CommandMenuProviderProps | undefined>(undefined)

const useCommandMenu = () => {
  const context = use(CommandMenuContext)

  if (!context) {
    throw new Error("useCommandMenu must be used within a <CommandMenuProvider />")
  }

  return context
}

interface CommandMenuProps extends AutocompleteProps, MenuTriggerProps, CommandMenuProviderProps {
  isDismissable?: boolean
  "aria-label"?: string
  shortcut?: string
  isBlurred?: boolean
  classNames?: {
    overlay?: string
    content?: string
  }
}

const CommandMenu = ({
  onOpenChange,
  classNames,
  isDismissable = true,
  escapeButton = true,
  isPending,
  isBlurred,
  shortcut,
  ...props
}: CommandMenuProps) => {
  const { contains } = useFilter({ sensitivity: "base" })
  const filter = (textValue: string, inputValue: string) => contains(textValue, inputValue)
  useEffect(() => {
    if (!shortcut) return

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === shortcut && (e.metaKey || e.ctrlKey)) {
        onOpenChange?.(true)
      }
    }

    document.addEventListener("keydown", onKeyDown)
    return () => document.removeEventListener("keydown", onKeyDown)
  }, [shortcut, onOpenChange])
  return (
    <CommandMenuContext value={{ isPending: isPending, escapeButton: escapeButton }}>
      <ModalContext value={{ isOpen: props.isOpen, onOpenChange: onOpenChange }}>
        <ModalOverlay
          isDismissable={isDismissable}
          className={twMerge([
            "fixed inset-0 z-50 max-h-(--visual-viewport-height) bg-black/15 dark:bg-black/40",
            "data-entering:fade-in data-exiting:fade-out data-entering:animate-in data-exiting:animate-in",
            isBlurred && props.isOpen ? "backdrop-blur" : "",
            classNames?.overlay ?? "",
          ])}
        >
          <Modal
            className={twMerge([
              "fixed top-auto bottom-0 start-[50%] z-50 grid h-[calc(100vh-30%)] w-full max-w-full translate-x-[-50%] gap-4 overflow-hidden rounded-t-2xl bg-overlay text-overlay-fg shadow-lg ring-1 ring-fg/10 sm:top-[6rem] sm:bottom-auto sm:h-auto sm:w-full sm:max-w-2xl sm:rounded-xl dark:ring-border forced-colors:border",
              "data-entering:fade-in-0 data-entering:slide-in-from-bottom sm:data-entering:slide-in-from-bottom-0 sm:data-entering:zoom-in-95 data-entering:animate-in data-entering:duration-300 sm:data-entering:duration-300",
              "data-exiting:fade-out sm:data-exiting:zoom-out-95 data-exiting:slide-out-to-bottom-56 sm:data-exiting:slide-out-to-bottom-0 data-exiting:animate-out data-exiting:duration-200",
              classNames?.content ?? "",
            ])}
            {...props}
          >
            <Dialog
              aria-label={props["aria-label"] ?? "Command Menu"}
              className="overflow-hidden outline-hidden"
            >
              <Autocomplete filter={filter} {...props} />
            </Dialog>
          </Modal>
        </ModalOverlay>
      </ModalContext>
    </CommandMenuContext>
  )
}

interface CommandMenuSearchProps extends SearchFieldProps {
  placeholder?: string
  className?: string
}

const CommandMenuSearch = ({ className, placeholder, ...props }: CommandMenuSearchProps) => {
  const state = use(OverlayTriggerStateContext)!
  const { isPending, escapeButton } = useCommandMenu()
  return (
    <SearchField
      aria-label="Quick search"
      autoFocus
      className={composeTailwindRenderProps(
        className,
        "flex w-full items-center border-b px-2.5 py-1",
      )}
      {...props}
    >
      {isPending ? (
        <Loader className="size-4.5" variant="spin" />
      ) : (
        <IconSearch
          data-slot="command-menu-search-icon"
          className="size-5 shrink-0 text-neutral-content"
        />
      )}
      <Input
        placeholder={placeholder ?? "Search..."}
        className="w-full min-w-0 bg-transparent px-2.5 py-2 text-base text-neutral-content placeholder-muted-fg outline-hidden focus:outline-hidden sm:text-sm [&::-ms-reveal]:hidden [&::-webkit-search-cancel-button]:hidden"
      />
      {escapeButton && (
        <Button
          onPress={() => state?.close()}
          className="hidden cursor-pointer rounded border text-current/90 hover:bg-muted lg:inline lg:px-1.5 lg:py-0.5 lg:text-xs"
        >
          Esc
        </Button>
      )}
    </SearchField>
  )
}

const CommandMenuList = <T extends object>({ className, ...props }: MenuProps<T>) => {
  return (
    <CollectionRendererContext.Provider value={renderer}>
      <MenuPrimitive
        className={composeTailwindRenderProps(
          className,
          "grid max-h-full grid-cols-[auto_1fr] overflow-y-auto p-2 sm:max-h-110 *:[[role=group]]:mb-6 *:[[role=group]]:last:mb-0",
        )}
        {...props}
      />
    </CollectionRendererContext.Provider>
  )
}

const CommandMenuSection = <T extends object>({
  className,
  ref,
  ...props
}: MenuSectionProps<T>) => {
  return (
    <MenuSection
      ref={ref}
      className={twMerge(
        className,
        "col-span-full grid grid-cols-[auto_1fr] gap-y-[calc(var(--spacing)*0.25)]",
      )}
      {...props}
    >
      {"title" in props && (
        <Header className="col-span-full mb-1 block min-w-(--trigger-width) truncate px-2.5 text-neutral-content text-xs">
          {props.title}
        </Header>
      )}
      <Collection items={props.items}>{props.children}</Collection>
    </MenuSection>
  )
}

const CommandMenuItem = ({ className, ...props }: React.ComponentProps<typeof Menu.Item>) => {
  const textValue =
    props.textValue || (typeof props.children === "string" ? props.children : undefined)
  return (
    <Menu.Item
      {...props}
      textValue={textValue}
      className={composeTailwindRenderProps(className, "gap-y-0.5 px-2.5 py-2")}
    />
  )
}

interface CommandMenuDescriptionProps extends React.ComponentProps<"span"> {
  intent?: "danger" | "warning" | "primary" | "secondary" | "success"
}

const CommandMenuDescription = ({ intent, className, ...props }: CommandMenuDescriptionProps) => {
  return (
    <span
      {...props}
      slot="command-menu-description"
      className={twMerge(
        "ms-auto hidden text-sm sm:inline",
        intent === "danger"
          ? "text-danger/90 group-selected:text-neutral-content/70"
          : intent === "warning"
            ? "text-warning/90 group-selected:text-neutral-content/70"
            : intent === "success"
              ? "text-success/90 group-selected:text-neutral-content/70"
              : intent === "primary"
                ? "text-neutral-content/90 group-selected:text-white/70"
                : "text-neutral-content group-selected:text-neutral-content/70",
        className,
      )}
    />
  )
}

const renderer: CollectionRenderer = {
  CollectionRoot(props) {
    if (props.collection.size === 0) {
      return (
        // biome-ignore lint/a11y/useFocusableInteractive: <explanation>
        <div role="menuitem" className="col-span-full p-4 text-center text-neutral-content text-sm">
          No results found.
        </div>
      )
    }
    return <DefaultCollectionRenderer.CollectionRoot {...props} />
  },
  CollectionBranch: DefaultCollectionRenderer.CollectionBranch,
}

const CommandMenuSeparator = ({
  className,
  ...props
}: React.ComponentProps<typeof Menu.Separator>) => (
  <Menu.Separator className={twMerge("-mx-2", className)} {...props} />
)

const CommandMenuLabel = Menu.Label
const CommandMenuKeyboard = DropdownKeyboard
CommandMenu.Search = CommandMenuSearch
CommandMenu.List = CommandMenuList
CommandMenu.Item = CommandMenuItem
CommandMenu.Label = CommandMenuLabel
CommandMenu.Section = CommandMenuSection
CommandMenu.Description = CommandMenuDescription
CommandMenu.Keyboard = CommandMenuKeyboard
CommandMenu.Separator = CommandMenuSeparator

export type { CommandMenuProps, CommandMenuSearchProps, CommandMenuDescriptionProps }
export { CommandMenu }
